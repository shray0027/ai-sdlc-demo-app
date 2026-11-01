# Narrow independent specification recheck

Scope: SPEC-01 from `docs/agents/specification-review.md` and the public JSON naming clarification only. The initial review is preserved. This is an independent agent specification recheck, not the workshop PR review, implementation verification or a participant verdict.

## SPEC-01 result

Addressed in the planning artifacts. `openspec/changes/invoice-approval-limits/tasks.md:25` now explicitly replaces only the temporary stage-one above-500,000 workflow-unavailable denial tests with first/second-approval expectations, retaining every lower-value role/limit, amount, identity and full-state regression. `tasks.md:30` now requires the suite with those retained regressions and the expressly superseding high-value transition tests. `openspec/changes/invoice-approval-limits/design.md:61` gives the same replacement and retention instructions. The generic stage-two delegation text at `tasks.md:23` is consistent with these explicit task and design details. This resolves the initial ambiguity without weakening either standalone stage's acceptance expectations.

## Public JSON names

`openspec/changes/invoice-approval-limits/specs/invoice-approval/spec.md:121` now names `firstApprovedBy`, `firstApprovedRole`, `firstApprovedAt`, `decidedBy` and `decidedAt`; its first-approval scenario at line 126 uses `decidedBy`, `decidedAt` and `decisionNote`. These match the camelCase JSON design described at `design.md:27`, while the design retains PascalCase names for C# properties. This is a serialization naming clarification and does not change eligibility, limits, identity, precision, lifecycle or other approval policy.

## Evidence limits and actual tools

Used the already-read repository `invoice-assignment` skill in review mode. Actual tools for this recheck: `functions.exec` with `tools.exec_command`, running `rg` and `sed` to inspect the three scoped artifacts and `cat` with a shell heredoc to write this result. No tests, OpenSpec validation, browser, production edits, OpenSpec edits, log edits, commits or sub-agents were used. No participant disposition is supplied; human judgments remain the participant's responsibility.
