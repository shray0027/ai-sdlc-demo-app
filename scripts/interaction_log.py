#!/usr/bin/env python3
"""Local-only Codex lifecycle capture and explicit assignment transcript export."""
import argparse
from datetime import datetime, timezone
import fcntl
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
LOGS = ROOT / "logs"


def now():
    return datetime.now(timezone.utc).isoformat()


def sanitize(value):
    if isinstance(value, dict):
        return {k: ("[REDACTED]" if re.search(
            r"^(?:token|access_token|api_key|authorization|password|secret)$", k, re.I
        ) else sanitize(v)) for k, v in value.items()}
    if isinstance(value, list):
        return [sanitize(v) for v in value]
    if isinstance(value, str):
        value = re.sub(r"\b(?:gh[pousr]_[A-Za-z0-9_]{10,}|github_pat_[A-Za-z0-9_]+|sk-[A-Za-z0-9_-]{16,})", "[REDACTED]", value)
        value = re.sub(r"(?i)(Bearer\s+)[A-Za-z0-9._~-]+", r"\1[REDACTED]", value)
        return re.sub(r'(?i)(["\']?(?:token|access_token|api_key|password|secret)["\']?\s*[:=]\s*)["\'][^"\'\n]+["\']', r'\1"[REDACTED]"', value)
    return value


def append_unique(path, records):
    path.parent.mkdir(parents=True, exist_ok=True)
    count = 0
    with path.open("a+", encoding="utf-8") as stream:
        fcntl.flock(stream, fcntl.LOCK_EX)
        stream.seek(0)
        known = {json.loads(line)["id"] for line in stream if line.strip()}
        stream.seek(0, 2)
        for record in records:
            if record["id"] not in known:
                stream.write(json.dumps(sanitize(record), ensure_ascii=False) + "\n")
                known.add(record["id"])
                count += 1
    return count


def read_events(path):
    # A live transcript may end in an incomplete line; retry it at the next sync.
    events = []
    with path.open(encoding="utf-8") as stream:
        for line in stream:
            if not line.endswith("\n"):
                break
            events.append(json.loads(line))
    return events


def sync(path):
    events = read_events(path)
    meta = next((e["payload"] for e in events if e.get("type") == "session_meta"), None)
    if not meta:
        raise ValueError("No session_meta found; unsupported transcript format")
    cwd = Path(meta.get("cwd", "/")).resolve()
    if cwd not in (ROOT, ROOT.parent):
        raise ValueError("Refusing to export a session outside this assignment workspace")
    session = meta["id"]
    if not re.fullmatch(r"[A-Za-z0-9_-]+", session):
        raise ValueError("Unsafe session identifier")
    model = None
    turn = None
    records = []
    for index, event in enumerate(events):
        payload = event.get("payload", {})
        if event.get("type") == "turn_context":
            model, turn = payload.get("model"), payload.get("turn_id")
            continue
        if event.get("type") != "response_item":
            continue
        kind = payload.get("type")
        data = None
        if kind == "message" and payload.get("role") in ("user", "assistant"):
            if payload.get("channel") == "analysis":
                continue
            content = [c.get("text", "") for c in payload.get("content", [])
                       if c.get("type") in ("input_text", "output_text", "text")]
            if content:
                data = {"role": payload["role"], "text": "\n".join(content)}
        elif kind in ("function_call", "custom_tool_call"):
            data = {"name": payload.get("name"), "call_id": payload.get("call_id"),
                    "input": payload.get("arguments", payload.get("input"))}
        elif kind in ("function_call_output", "custom_tool_call_output"):
            data = {"call_id": payload.get("call_id"), "output": payload.get("output")}
        if data is None:
            continue  # Never export reasoning, encrypted content, or system/developer messages.
        records.append({"id": f"{session}:{index}", "timestamp": event.get("timestamp"),
                        "capture": "automatic_transcript_export", "source_line": index + 1,
                        "source_file": path.name, "session_id": session, "turn_id": turn,
                        "model": model, "ai_tool": "Codex", "source": meta.get("source"),
                        "event_type": kind, "data": data})
    target = LOGS / "transcripts" / f"{session}.jsonl"
    count = append_unique(target, records)
    return {"session_id": session, "exported": count, "selected_records": len(records),
            "artifact": str(target.relative_to(ROOT)), "last_observed_model": model}


def hook():
    payload = json.load(sys.stdin)
    event = payload.get("hook_event_name")
    if not event or not payload.get("session_id"):
        raise ValueError("Expected an actual Codex hook event with session_id")
    fingerprint = hashlib.sha256(json.dumps(payload, sort_keys=True).encode()).hexdigest()[:20]
    record = {"id": "hook-" + fingerprint, "timestamp": now(), "capture": "native_lifecycle_hook",
              "event": event, "session_id": payload["session_id"], "model": payload.get("model"),
              "payload": payload}
    append_unique(LOGS / "lifecycle.jsonl", [record])
    # Transcript may lag a hook; a final explicit sync preserves the latest output.
    transcript = payload.get("transcript_path")
    if transcript and Path(transcript).is_file():
        sync(Path(transcript))
    print("{}")  # Logging never requests continuation or changes approval behavior.


def summary():
    entries = read_events(LOGS / "interactions.jsonl")
    hooks_path = LOGS / "lifecycle.jsonl"
    hooks = read_events(hooks_path) if hooks_path.exists() else []
    transcripts = sorted((LOGS / "transcripts").glob("*.jsonl"))
    lines = ["# AI interaction summary", "", "Generated from structured records; human validation is only reported where explicitly supplied.", ""]
    for entry in entries:
        request = entry.get('request', '')
        # Prompts may contain relative Markdown links or code; render their text literally.
        fence = "`" * max(3, max((len(m.group()) + 1 for m in re.finditer(r"`+", request)), default=3))
        lines += [f"## {entry['id']}", "", f"- Capture: {entry.get('capture', 'agent-authored task record')}",
                  f"- Outcome: {entry.get('outcome', '')}", "", "Request:", "",
                  fence + "text", request, fence, ""]
    lines += [f"Native lifecycle events recorded: {len(hooks)}.", "", "## Transcript exports", ""]
    for path in transcripts:
        records = read_events(path)
        lines.append(f"- [{path.stem}](transcripts/{path.name}): {len(records)} selected records.")
    lines += ["", "Coverage, redaction and activation limits are described in [README.md](README.md).", ""]
    (LOGS / "SUMMARY.md").write_text("\n".join(lines))
    print(json.dumps({"task_records": len(entries), "lifecycle_events": len(hooks), "transcripts": len(transcripts)}))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("hook")
    sync_parser = commands.add_parser("sync")
    sync_parser.add_argument("--session", required=True, type=Path)
    commands.add_parser("summary")
    record_parser = commands.add_parser("record")
    record_parser.add_argument("--file", required=True, type=Path)
    args = parser.parse_args()
    if args.command == "hook":
        hook()
    elif args.command == "sync":
        print(json.dumps(sync(args.session)))
    elif args.command == "summary":
        summary()
    else:
        record = json.loads(args.file.read_text())
        required = {"id", "timestamp", "request", "ai_tool", "model", "agent", "skills_used",
                    "tools_used", "actions", "human_intervention", "evidence", "outcome",
                    "related_ids", "parent_task", "capture"}
        missing = required - record.keys()
        if missing:
            raise ValueError(f"Task record missing fields: {sorted(missing)}")
        record.setdefault("commit_at_capture", subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip())
        print(json.dumps({"appended": append_unique(LOGS / "interactions.jsonl", [record])}))


if __name__ == "__main__":
    main()
