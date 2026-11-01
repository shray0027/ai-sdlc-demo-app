# Nine-minute live demo

Record an 8–10-minute MP4 and upload it using your chosen recorder/storage. This runbook is preparation; it is not a recording or an uploaded submission. The participant will record/upload and share the link.

## Prepare before recording

Open the application repository in the terminal. Keep the PR, ASSIGNMENT.md, AGENTS.md, the OpenSpec spec, the finding ledger and mutation summary ready in editor/browser tabs. Use a readable font and keep the terminal output visible while commands execute.

```sh
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
python3 scripts/demo_rehearsal.py --serve
```

The helper starts its own app on a printed free loopback port, performs real API approval/immutability checks, then creates three fresh PendingApproval invoices for USD 50,000.00, 50,000.01 and 500,000.01. Open the printed URL. Keep this terminal running; Ctrl+C stops only this owned app and discards its in-memory data. It never reuses your app on port 5000. If using an installed .NET 8 SDK instead, put `dotnet` on PATH and set DOTNET_ROOT for that installation rather than the local path above.

Use the public demo accounts from AGENTS.md: Manager `demo.approver`; Directors `demo.director1` and `demo.director2`; password `ChangeMe123!`. Sign out between identities. No session token needs to be displayed. The UI data is synthetic and reset on restart. Test before filming: `python3 scripts/demo_rehearsal.py` runs the checks and stops automatically; `--serve` leaves UI seeds ready.

## Recording sequence

| Time | Show and explain | Evidence to have ready |
| --- | --- | --- |
| 0:00–0:50 | State FR-01–FR-05, inclusive thresholds and the two participant choices: USD cents and Manager-or-Director first eligibility. | ASSIGNMENT.md; docs/decisions/approval-policy.md |
| 0:50–1:40 | Show AGENTS layering/test rules, actual agent hierarchy, a structured interaction and its automatic transcript link. Explain the IDE hook activation limit honestly. | AGENTS.md; docs/agents/README.md; logs/SUMMARY.md; logs/README.md |
| 1:40–2:15 | Show generated OpenSpec spec/tasks and the two separate implementation commits. Explain that 26/26 engineering tasks does not finish the video requirement. | OpenSpec spec/tasks; `git show --stat 5cf4d61`; `git show --stat df5fa14` |
| 2:15–4:10 | Perform the three seeded UI flows below. Reload after rejection to show persisted state. Explain that automated tests compare all 11 fields; UI status alone is insufficient. | App at the helper's printed URL |
| 4:10–5:30 | Run the .NET suite and real Chromium test live using the commands below. Leave actual execution/results on screen; do not replace this with a file read-through. | Second terminal; existing E2E test/page object |
| 5:30–6:30 | Show literal BEFORE 100.00% (97 kills/3 timeouts) and AFTER 100.00% (100 kills/0 timeouts). Explain unchanged score, matching scope/production, automatic ignored mutants and meaningful error assertion fixes. | Mutation README; status-counts.json; strengthened-mutant-detection.json |
| 6:30–7:40 | Show exact workshop prompt, independent report with no new findings, and all nine earlier findings marked Fixed by you. Show concrete JSON/form precision fixes or one strengthened test. | PR #1; docs/agents/workshop-review.md; docs/evidence/phase-7/FINDINGS.md |
| 7:40–9:00 | Show write-up, CI, artifact checklist, limitations and submission links. State that the recording link will be inserted after upload. | ASSIGNMENT.md; docs/SUBMISSION_CHECKLIST.md |

### Seeded UI flows

1. Sign in as Manager. Approve the synthetic USD 50,000.00 invoice: Approved.
2. Attempt Manager approval on synthetic USD 50,000.01: visible Manager limit error; reload and show PendingApproval. Later, a Director may approve this invoice.
3. Sign in as Director 1 and approve synthetic USD 500,000.01 once: still PendingApproval with first-approval history. Try again as Director 1: distinct-user error, progress unchanged. Sign out, sign in as Manager and attempt the second approval: distinct-Director error. Sign out, sign in as Director 2 and approve: Approved.

The helper prints invoice IDs. Use the three `UI seed` rows, rather than earlier rehearsal invoices already approved. If a flow is consumed during practice, restart the helper to obtain fresh seeds.

### Live test commands

In another terminal at the application repository root with the SDK environment set:

```sh
dotnet test backend/InvoiceApp.sln --configuration Release
node --test backend/tests/ui/*.test.cjs
```

Expected current results: 173 .NET and 46 Node tests. For the real Chromium case:

```sh
cd e2e
npm ci
npx playwright install chromium
npm test -- --reporter=list
```

Install dependencies/browser before recording if needed; run the final test live during recording. Current expected result: 1 passed. Playwright owns a different port and stops its own server. The test writes `docs/evidence/phase-6/playwright/invoice-state.json` for the new execution even with list-only reporting; this creates a local evidence diff. Keep that output as your recording-run evidence or restore only that known generated file after saving it elsewhere. Do not commit video, screenshots or unrelated local changes. Node 24 was used for verification; CI supplies it.

### After recording/upload

- Confirm the MP4 is 8–10 minutes and contains readable live execution.
- Upload it to your chosen host and use reviewer-accessible sharing.
- Open the share link in a signed-out/private browser and check playback/access. Record if organizational access is intentionally required.
- Share the URL here. The agent can add it to ASSIGNMENT.md and the PR, check accessible metadata where supported, and update the final checklist; it cannot honestly certify private playback without access.

PR: https://github.com/shray0027/ai-sdlc-demo-app/pull/1. Upload the MP4 outside Git; source images remain excluded too.
