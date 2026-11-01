# Independent specification review — Phase 3

This is an independent specification review of `openspec/changes/invoice-approval-limits/`, performed against the current repository and assignment requirements. It is not the exact workshop PR/diff review, implementation verification, generated-test participant review, or a participant verdict. Human finding dispositions remain pending.

## Overall assessment

The proposal, spec, design and task breakdown preserve FR-01–FR-05 and provide meaningful coverage for inclusive/adjacent thresholds, distinct canonical identity across sessions and roles, Director-second authority, complete failed-attempt immutability, and concurrent transitions. Participant choices (Manager or Director first; positive USD cents) are distinguished from agent-selected role migration, representation and atomicity design. Domain rules, trusted API authority, repository isolation and UI error/progress behavior are assigned to the appropriate layers. Separate stage-one and stage-two prompts and commits are planned, with the intermediate high-value denial explicitly identified as temporary rather than FR-03 completion.

One small task clarification should be made before implementation. No blocking contradiction or weakening of the five required rules was found. No implementation, test execution, human generated-test review, mutation result, workshop review, or publication completion is claimed by the reviewed planning artifacts.

## Findings

### SPEC-01 — Clarify replacement of temporary stage-one high-value rejection tests (low)

**Paths/evidence:** `openspec/changes/invoice-approval-limits/tasks.md:14` requires temporary high-value immutable rejection tests; `tasks.md:23` instructs the second agent to rerun stage-one regressions; `tasks.md:25` adds successful first-approval cases; `tasks.md:30` requires rerunning the full suite including stage one. Design decision 6 describes the intentionally different intermediate behavior.

**Issue:** A stage-one test asserting an eligible Manager/Director receives an unavailable-workflow error and no state change above 500,000 is intentionally incompatible with stage-two first-approval success. The task plan identifies it as temporary but does not explicitly tell the second implementer to replace that expectation while preserving lower-value regressions. Leaving both expectations in the final suite would prevent it from passing; indiscriminately dropping stage-one cases could weaken lower-value coverage.

**Suggested correction:** In the stage-two prompt/task, explicitly supersede only the temporary workflow-unavailable high-value assertions with the final pending-first-approval scenarios. Retain all lower-value limits, invalid input, role, repository isolation and immutability cases, extending complete snapshots for the new fields. The original temporary tests remain valid evidence in the standalone stage-one commit.

**Participant verdict:** Pending. This recommendation is an agent review finding, not a human disposition.

## Review scope and actual tools

Read `AGENTS.md`, Phase 3 and staged implementation/testing/review requirements in `ACTION_PLAN.md`, `.agents/skills/invoice-assignment/SKILL.md` (review mode), `requirements/REQUIREMENTS.md`, `docs/decisions/approval-policy.md`, and all five change files (`proposal.md`, spec delta, `design.md`, `tasks.md`, `.openspec.yaml`). Traced the planned change through current Domain `ApprovalService`, `Invoice`, `IInvoiceRepository`; API invoice/approval/base/auth controllers; auth middleware, role filter, sessions and demo configuration; `InMemoryInvoiceRepository`; static `app.js`/`index.html`; existing xUnit service tests/fake/project references; and Playwright configuration. Read the agent-responsibility context to distinguish actual and planned review work.

Actual tools: `functions.exec` with `tools.exec_command` for read-only shell inspection (`pwd`, `rg`, `cat`, `sed`, `nl`, `git status --short`) and writing this review file; `collaboration.send_message` for the coordinator handoff. Actual skill used: repository `invoice-assignment`, review mode. No sub-agents were spawned. No browser, web search, app tests, mutation runs, structural OpenSpec validation, production changes, OpenSpec edits or commits were performed. The coordinator owns structural validation and finding handling.
