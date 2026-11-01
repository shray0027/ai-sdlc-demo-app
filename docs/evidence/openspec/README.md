# OpenSpec artifact-generation evidence

Change: `invoice-approval-limits`. Executed 2026-11-01 by delegated specification task `/root/specification`, parent `/root`. Model/session identifiers were not observed. This records planning generation, not implementation, test execution or participant approval.

Actual skills read/applied: `.agents/skills/invoice-assignment/SKILL.md` (Specification) and generated `.agents/skills/openspec-propose/SKILL.md` (generatedBy 1.16.0). Root AGENTS.md, ACTION_PLAN Phase 3, requirements, source/tests/UI/auth and the resolved root config were inspected. Tools used: functions.exec → tools.exec_command (shell, cat, rg, sort, Python path/content checks, repository-local OpenSpec CLI); collaboration.send_message for coordinator progress. No additional agents, web/browser tools, apply workflow, production edits, tests or commits were used.

All CLI commands ran from `/Users/shrayanand/Documents/learnings/NAGP/ai-sdlc-assignment/ai-sdlc-demo-app` with:

```sh
export PATH="/Users/shrayanand/.nvm/versions/node/v24.18.0/bin:$PATH"
export OPENSPEC_TELEMETRY=0
```

The executable was `.tools/openspec/node_modules/.bin/openspec`. Raw combined stdout/stderr is preserved per command:

| Output | Arguments after executable |
| --- | --- |
| 01-version.txt | --version |
| 02-project-list.json | list --json |
| 03-context.json | context --json |
| 04-new-change.txt | new change invoice-approval-limits |
| 05-status-initial.json | status --change invoice-approval-limits --json |
| 06-instructions-proposal.json | instructions proposal --change invoice-approval-limits --json |
| 07-capability-inventory.txt | list --specs |
| 08-status-after-proposal.json | status --change invoice-approval-limits --json |
| 09-instructions-specs.json | instructions specs --change invoice-approval-limits --json |
| 10-status-after-specs.json | status --change invoice-approval-limits --json |
| 11-instructions-design.json | instructions design --change invoice-approval-limits --json |
| 12-status-after-design.json | status --change invoice-approval-limits --json |
| 13-instructions-tasks.json | instructions tasks --change invoice-approval-limits --json |
| 14-status-after-tasks.json | status --change invoice-approval-limits --json |
| 15-validation-strict.txt | validate invoice-approval-limits --strict |
| 16-status-final.txt | status --change invoice-approval-limits |
| 17-status-final.json | status --change invoice-approval-limits --json |

The project check and context resolved this repository as the root; no registered standalone store was selected. The CLI created the change scaffold. Following the actual artifact instructions, the agent authored proposal, the new invoice-approval delta, design and tasks in dependency order, rereading completed dependencies from disk. The new spec path was resolved/checked inside changeRoot before creation. No conditional artifact was skipped. Status was rerun after each artifact.

Observed results: CLI version 1.16.0; no preexisting capability specs; strict validation output `Change 'invoice-approval-limits' is valid`; final status `4/4 artifacts complete`. `isComplete` in the CLI status refers here to planning-file existence, not completion of implementation or verification. At generation time every task was unchecked. Independent specification review then identified SPEC-01, which the coordinator clarified and the reviewer rechecked. Only planning-review task 1.1 is now checked; all implementation/test-execution tasks and participant test/review judgments remain pending. The final submitted planning state is in outputs 20–21.

Artifacts are under `openspec/changes/invoice-approval-limits/`: proposal.md, specs/invoice-approval/spec.md, design.md, tasks.md and the CLI scaffold .openspec.yaml. All artifacts needed for implementation planning are ready for review. Starting implementation requires a subsequent apply request; this task did not invoke apply.

Remaining limitations for review: agent-selected legacy role/intentional rejection migration; username identity remains tied to fixed demo configuration; the global lock is local-process only; JavaScript large-number transport remains a disclosed UI limitation; no unit/API/browser/mutation execution is claimed. The participant's two policy answers do not constitute approval of the generated artifacts or tests.

## Post-generation review checks

The coordinator preserved initial artifact hashes in `reviewed-artifact-hashes.json`, addressed SPEC-01 and clarified public JSON casing, and reran strict validation. Outputs `18-validation-after-review.txt` and `19-status-after-review.json` describe that result. After recording the completed planning-review task and review provenance, `20-validation-submitted.txt` and `21-status-submitted.json` record the final submitted planning state. No code or tests were implemented.
