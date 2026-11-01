#!/usr/bin/env python3
"""Run real HTTP demo scenarios on an owned server; --serve leaves seeded UI ready."""
import argparse
import json
import os
from pathlib import Path
import shutil
import signal
import socket
import subprocess
import tempfile
import time
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen
from uuid import uuid4

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--serve', action='store_true', help='Keep the owned app running until Ctrl+C')
    args = parser.parse_args()
    local_sdk = ROOT / '.tools/dotnet'
    dotnet = str(local_sdk / 'dotnet') if (local_sdk / 'dotnet').is_file() else shutil.which('dotnet')
    if not dotnet:
        parser.error('Install .NET 8 SDK or restore the local .tools/dotnet SDK.')
    env = os.environ.copy()
    if (local_sdk / 'dotnet').is_file():
        env['DOTNET_ROOT'] = str(local_sdk)
        env['PATH'] = str(local_sdk) + os.pathsep + env.get('PATH', '')
    with socket.socket() as probe:
        probe.bind(('127.0.0.1', 0))
        port = probe.getsockname()[1]
    base = f'http://127.0.0.1:{port}'

    def request(method, route, token=None, payload=None, expected=200):
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer ' + token
        data = None if payload is None else json.dumps(payload).encode()
        req = Request(base + route, data=data, headers=headers, method=method)
        try:
            with urlopen(req, timeout=5) as response:
                code, body = response.status, response.read()
        except HTTPError as error:
            code, body = error.code, error.read()
        if code != expected:
            raise RuntimeError(f'{method} {route}: expected HTTP {expected}, got {code}')
        return json.loads(body) if body else None

    def login(username):
        # Public demo fixture only. Session tokens stay in memory and are never printed.
        return request('POST', '/api/auth/login', payload={
            'username': username, 'password': 'ChangeMe123!',
        })['token']

    def create(amount, label):
        invoice = request('POST', '/api/invoices', manager, {
            'customerName': f'Synthetic demo {label} {uuid4().hex[:8]}', 'amount': amount,
        }, expected=201)
        return request('POST', f"/api/invoices/{invoice['id']}/submit", manager)

    def approve(invoice, token):
        return request('POST', f"/api/approvals/{invoice['id']}/approve", token)

    def denied_unchanged(invoice, token, reason):
        before = request('GET', f"/api/invoices/{invoice['id']}", token)
        error = request('POST', f"/api/approvals/{invoice['id']}/approve", token, expected=409)
        assert reason.lower() in error['error'].lower(), error['error']
        after = request('GET', f"/api/invoices/{invoice['id']}", token)
        assert len(before) == 11 and before == after, 'Complete persisted invoice changed on rejection'
        return error['error']

    with tempfile.TemporaryFile(mode='w+') as server_log:
        process = subprocess.Popen([
            dotnet, 'run', '--no-launch-profile', '--project', 'backend/src/InvoiceApp.Api',
            '--', '--urls', base,
        ], cwd=ROOT, env=env, stdout=server_log, stderr=subprocess.STDOUT, start_new_session=True)
        try:
            deadline = time.monotonic() + 60
            while True:
                if process.poll() is not None:
                    raise RuntimeError('Owned demo server exited before startup; no existing server was reused')
                server_log.seek(0)
                listening = f'Now listening on: {base}' in server_log.read()
                if listening:
                    try:
                        with urlopen(base, timeout=1) as response:
                            if response.status == 200:
                                break
                    except (URLError, TimeoutError):
                        pass
                if time.monotonic() >= deadline:
                    raise TimeoutError('Owned demo server did not become ready within 60 seconds')
                time.sleep(0.1)
            print(f'Owned demo app: {base}', flush=True)
            manager, director1, director2 = (login(name) for name in (
                'demo.approver', 'demo.director1', 'demo.director2'))
            for amount, actor, label in [(50000, manager, 'Manager inclusive'), (500000, director1, 'Director inclusive')]:
                result = approve(create(amount, label), actor)
                assert result['status'] == 'Approved'
                print(f'PASS {label}: USD {amount:,.2f} approved', flush=True)
            invoice = create(50000.01, 'Manager over limit')
            reason = denied_unchanged(invoice, manager, '50,000')
            print(f'PASS Manager over limit: HTTP 409; all 11 fields unchanged; {reason}', flush=True)
            for first_role, first in [('Manager', manager), ('Director', director1)]:
                invoice = create(500000.01, first_role + ' first')
                first_state = approve(invoice, first)
                assert first_state['status'] == 'PendingApproval'
                assert first_state['firstApprovedRole'] == first_role
                assert all(first_state[k] is None for k in ('decidedBy', 'decidedAt', 'decisionNote'))
                second = director1 if first_role == 'Manager' else director2
                if first_role == 'Director':
                    denied_unchanged(invoice, login('demo.director1'), 'same user')
                    denied_unchanged(invoice, manager, 'distinct Director')
                    print('PASS duplicate Director after new login and distinct Manager second: HTTP 409; all 11 fields unchanged', flush=True)
                final = approve(invoice, second)
                assert final['status'] == 'Approved'
                assert final['firstApprovedBy'] != final['decidedBy']
                assert all(final[k] == first_state[k] for k in ('firstApprovedBy', 'firstApprovedRole', 'firstApprovedAt'))
                print(f'PASS {first_role} first + distinct Director second: Approved; first history preserved', flush=True)
            print('Rehearsal passed against the real API; session tokens were not printed.', flush=True)
            if args.serve:
                for amount, label in [(50000, 'UI inclusive'), (50000.01, 'UI rejection'), (500000.01, 'UI two-person')]:
                    seeded = create(amount, label)
                    print(f"UI seed: {seeded['id']} | USD {amount:,.2f} | PendingApproval", flush=True)
                print('Open the URL above. Use the demo accounts in AGENTS.md. Ctrl+C stops only this app.', flush=True)
                while process.poll() is None:
                    time.sleep(0.5)
                raise RuntimeError('Demo server exited unexpectedly')
        except KeyboardInterrupt:
            print('\nStopping owned demo app.', flush=True)
        finally:
            # dotnet run has a child application: terminate our own process group only.
            try:
                os.killpg(process.pid, signal.SIGTERM)
            except ProcessLookupError:
                pass
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                os.killpg(process.pid, signal.SIGKILL)
                process.wait(timeout=5)
            print('Owned demo server stopped; in-memory demo data discarded.', flush=True)


if __name__ == '__main__':
    main()
