# Interaction log audit

Scope: all current structured records and all exported JSONL files. Counts are a snapshot before the final evidence commit; later transcript syncs may append new events. This checks field presence, parsing, unique IDs and provenance structure; it is not an exhaustive semantic re-audit of every interaction.

- Structured task records: **27**.
- Automatic transcript files: **18**, containing **1280** selected events.
- Actual native lifecycle events: **5**; existing IDE hook activation remains unverified.
- Reconstructed initial record: `setup-001`, explicitly labeled; unknown historical model details stay qualified.

| Suggested category | Structural coverage | Concrete evidence |
| --- | --- | --- |
| AI tool and model | 27/27 records contain the field(s) | phase-7-workshop-review: observed gpt-6.1-sol and Codex; earliest setup labels exact model unknown |
| Agent details | 27/27 records contain the field(s) | phase-7-workshop-review and transcript metadata: independent task, parent and session identifiers |
| Prompts and interactions | 27/27 records contain the field(s) | exact workshop prompt and saved delegation; raw transcript tool calls/results |
| Skills and sub-agents | 27/27 records contain the field(s) | phase-7-review-completion: actual review skill and child relationship; no skill inferred from availability |
| Tools and integrations | 27/27 records contain the field(s) | actual per-task tools_used plus linked tool events; configured MCP is not proof of invocation |
| AI actions and outputs | 27/27 records contain the field(s) | actions with reports/source paths and raw execution outputs |
| Human intervention | 27/27 records contain the field(s) | phase-7-review-completion: exact nine-Fixed reply; Phase 8 exact participant recording choice |
| Execution evidence | 27/27 records contain the field(s) | timestamps, immutable revisions, 47 input hashes, test reports and source-line transcript provenance |
| Outcome | 27/27 records contain the field(s) | stage outcomes retain historical pending status; later acknowledgment records supply current completion |

Empty skill/sub-agent lists mean none was used for that record; not every interaction needs a child. Specific delegated tasks and parent/session IDs are captured in the task records/transcript source. Missing or unverified execution is not filled with sample values. Historical pending statements are not erased; later actual replies supply current statuses.

Each transcript entry parsed as JSON, has a unique ID, automatic-export capture label, source basename/positive source line, and matching session/file ID. Exported message roles are user/assistant. Logger filtering and activation/redaction limits are documented in [logs/README.md](../../../logs/README.md). No future completeness guarantee is inferred.

Artifacts: [structured task log](../../../logs/interactions.jsonl), [readable summary](../../../logs/SUMMARY.md), [machine-readable audit](log-audit.json).
