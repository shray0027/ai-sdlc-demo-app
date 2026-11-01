#!/usr/bin/env python3
"""Verify repository audit hooks in one read-only Codex task, without global config edits."""
import json
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_COMMAND = 'python3 "$(git rev-parse --show-toplevel)/scripts/interaction_log.py" hook'
EVENTS = {"SessionStart", "UserPromptSubmit", "PostToolUse", "SubagentStart", "SubagentStop", "Stop"}


def toml(value):
    if isinstance(value, dict):
        return "{" + ",".join(json.dumps(k) + "=" + toml(v) for k, v in value.items()) + "}"
    if isinstance(value, list):
        return "[" + ",".join(toml(v) for v in value) + "]"
    return json.dumps(value)


def main():
    definitions = json.loads((ROOT / ".codex/hooks.json").read_text())["hooks"]
    if set(definitions) != EVENTS:
        raise ValueError("Review changed hook event definitions before verification")
    for groups in definitions.values():
        for group in groups:
            if set(group) - {"matcher", "hooks"}:
                raise ValueError("Unexpected matcher-group settings")
            for hook in group["hooks"]:
                if hook != {"type": "command", "command": EXPECTED_COMMAND, "timeout": 30}:
                    raise ValueError("Verification only permits the reviewed local logger command")
    cli = shutil.which("codex")
    if not cli:
        raise RuntimeError("Codex CLI is not on PATH")
    command = [cli, "--no-daemon", "exec", "--json", "--sandbox", "read-only",
               "--dangerously-bypass-hook-trust"]
    for event, groups in definitions.items():
        command += ["-c", "hooks." + event + "=" + toml(groups)]
    command += ["--output-last-message", "docs/agents/logging-check-response.md", "-"]
    prefix = ROOT / "logs/logging-check-verified"
    Path(str(prefix) + "-command.json").write_text(json.dumps(command, indent=2) + "\n")
    with (ROOT / "docs/agents/logging-check-prompt.txt").open() as prompt, \
            Path(str(prefix) + "-events.jsonl").open("w") as output, \
            Path(str(prefix) + "-stderr.txt").open("w") as errors:
        result = subprocess.run(command, cwd=ROOT, stdin=prompt, stdout=output, stderr=errors)
    if result.returncode:
        raise SystemExit(result.returncode)
    rows = [json.loads(line) for line in Path(str(prefix) + "-events.jsonl").read_text().splitlines()]
    session = next(row["thread_id"] for row in rows if row.get("type") == "thread.started")
    lifecycle = ROOT / "logs/lifecycle.jsonl"
    delivered = {row["event"] for row in map(json.loads, lifecycle.read_text().splitlines())
                 if row["session_id"] == session} if lifecycle.exists() else set()
    required = {"SessionStart", "UserPromptSubmit", "PostToolUse", "Stop"}
    report = {"session_id": session, "required_events": sorted(required),
              "delivered_events": sorted(delivered), "passed": required <= delivered,
              "note": "No child agents requested; SubagentStart/Stop configured but not exercised."}
    Path(str(prefix) + "-result.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report))
    if not report["passed"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
