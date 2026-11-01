# Interaction capture and provenance

Three records serve different purposes:

- `interactions.jsonl`: agent-authored task metadata in JSONL. Original Phase 1 entry remains explicitly reconstructed. Human decisions are null/pending unless actually supplied.
- `transcripts/<session-id>.jsonl`: automatic exports from explicitly selected Codex saved rollout files. Each record identifies source file/line, original timestamp, session, observed model, turn and tool call ID. Includes available user/assistant text and tool calls/results; excludes system/developer messages, analysis-channel messages, reasoning and encrypted content. Unrelated workspace sessions are refused.
- `lifecycle.jsonl`: events actually delivered to the native repository hook. No fixture events go here. CLI verification event streams/stderr are separate `logging-check*` files, preserving both failed activation and successful delivery evidence where present.

`SUMMARY.md` is generated from these records. Actual verification outcomes are in `docs/evidence/PHASE_2.md`; delegated prompts/responses are in `docs/agents/`.

## Commands

From the application repository root, select the assignment transcript explicitly:

```sh
python3 scripts/interaction_log.py sync --session /absolute/path/to/rollout.jsonl
python3 scripts/interaction_log.py record --file /absolute/path/to/task-record.json
python3 scripts/interaction_log.py summary
python3 -m unittest discover -s scripts -p 'test_*.py' -v
```

`record` requires id, timestamp, request, ai_tool, model, agent, skills_used, tools_used, actions, human_intervention, evidence, outcome, related_ids, parent_task, and capture. Include exact prompts or prompt artifact paths, actual sub-agent relationships, affected files, tests and limitations. It attaches the Git revision at capture. Link metadata to raw session/call IDs rather than assert unsupported execution.

## Native hooks and activation

`.codex/config.toml` enables the hook feature at the project layer. `.codex/hooks.json` registers SessionStart, UserPromptSubmit, PostToolUse, SubagentStart, SubagentStop and Stop. Each invokes the Python logger on JSON stdin; it writes locally, exports the available transcript, and returns `{}`. It makes no permission or continuation decision and contacts no external service. Configuration and input fields follow [official Codex Hooks documentation](https://learn.chatgpt.com/docs/hooks).

Project hooks need a trusted project/config layer and reviewed hook definitions. A running IDE session may not reload newly written configuration, especially from the parent planning directory. File creation is not activation evidence. Phase 2 checks a new CLI session separately; it does not claim IDE hook delivery. For normal interactive use, open Codex at the application repository root and use `/hooks` to inspect/trust definitions. Continue the existing session using explicit transcript sync without changing account-wide settings.

Project-file discovery did not deliver hooks in the first two CLI checks, including an invocation-only project-trust override. An inline Stop definition delivered a real event. The reproducible verifier therefore supplies the repository definitions as invocation-only inline settings:

```sh
python3 scripts/verify_hooks.py
```

The verifier permits only the reviewed local logger command, records the exact CLI arguments, retains a read-only agent sandbox and changes no persisted global trust. It uses the documented hook-trust bypass for this vetted one-off automation. It checks actual SessionStart/UserPromptSubmit/PostToolUse/Stop delivery; child lifecycle events are configured but not exercised by this no-delegation prompt. See `logging-check-verified-result.json`. This is a scoped verification, not general approval bypass guidance.

## Coverage and limits

- Hooks append when delivered. A final explicit sync catches transcript output flushed after a hook. Sync is idempotent and retries a partial final line once complete. File locking protects concurrent appends.
- Saved transcript schema is an implementation detail, tested against CLI 0.162.0-alpha.17.2. Unknown formats fail visibly. A snapshot is not proof of future capture.
- Planning/current sessions are selected explicitly. Select child transcripts separately when available, otherwise preserve exact prompts/results and label the missing child tool transcript.
- Basic redaction covers credential fields and common token/Bearer patterns; it is best effort. Exported demo credentials can be redacted; source line references support local provenance checks.
- The nine assignment categories are covered across linked task records and raw evidence; each hook need not contain every category. Record skills/integrations only when used; do not infer human validation.
- No native hook delivery is claimed for Phase 1. History was exported later; the original reconstruction remains labeled. Raw old commit references describe history before the participant-requested image-removal amendment.
