# Phase 8 demo preparation review

Task `/root/workshop_review`, parent `/root`; follow-up instructions saved in `docs/agents/phase-eight-demo-review-prompt.txt`. Skill actually used: `.agents/skills/invoice-assignment/SKILL.md`, review/testing guidance. Applied the same participant prompt verbatim:

> review this PR against our checklist . Report findings by severity with file and line. suggest a fix for each, and say what you are unsure about.

Scope: new working-tree `scripts/demo_rehearsal.py` and `docs/DEMO_RUNBOOK.md`, plus supporting rehearsal evidence. Starting committed revision: `f5694efcc41b9022d89d610598ee2a9e22b24e87`. This is a narrow supplement to `workshop-review.md`, not a repeat review of application code. All 47 application/test/tool manifest entries remain unchanged. No child agent, commit, helper/source/test/log/evidence edit or browser/mutation rerun was performed. This report is the sole reviewer-owned output. Model/session identifier not independently observed.

## Findings by severity

**No actionable new findings.** No critical/high/medium/low defect was substantiated in the helper or runbook. No new P8 finding IDs, fixes or participant verdicts are invented.

An initially missing runbook reference, `docs/DEMO_RUNBOOK.md:30` → `docs/SUBMISSION_CHECKLIST.md`, was reported to the coordinator during review. The coordinator confirmed it was an in-progress handoff artifact and created it; existence was rechecked before completing review. It is not an outstanding finding.

## Assessment

- `scripts/demo_rehearsal.py:24` selects the available local SDK or installed dotnet; `:32` selects a loopback port. `:76` starts a new process session, then `:83`/`:86` requires the owned process to remain alive and emit its own listening marker before HTTP readiness. It does not silently attach to an existing port-5000 app.
- `scripts/demo_rehearsal.py:37` performs real HTTP requests and checks expected status codes. `:67` verifies both rejection wording and equality of fresh complete before/after persisted reads, including the 11-field count. Rehearsal amounts are ordinary representable cents, so Python JSON transport does not present the extreme monetary precision risks already covered by application tests.
- `scripts/demo_rehearsal.py:100` exercises both inclusive limits, Manager over-limit denial, both first roles, duplicate Director after a fresh login, distinct Manager second denial and valid distinct Director completion. `:110` checks pending first progress with null final fields; `:119` checks final status, distinct identity and preserved first metadata. This is useful demo verification, not a replacement for the complete Domain/API/concurrency suites.
- `scripts/demo_rehearsal.py:125` creates three new pending synthetic invoices after rehearsal, prints their IDs and leaves them available only with `--serve`. These match the runbook's usable UI flows. Earlier rehearsal invoices have distinct labels and IDs; the runbook explicitly tells the participant to use the fresh UI seeds.
- `scripts/demo_rehearsal.py:132` and `:134` handle Ctrl+C and finally terminate the owned process group, including the dotnet child app. The code waits and escalates on timeout. Recorded normal and interrupted runs both end with teardown; both recorded ports were closed during this review.
- `docs/DEMO_RUNBOOK.md:23` provides a nine-minute sequence with live execution, actual reviews and honest mutation-score interpretation. Its SDK/accounts/seeding instructions match the helper and repository. `:55` uses list-only reporting, while `:58` accurately warns that the test itself still writes the tracked invoice-state artifact. Its upload/playback tasks remain assigned to the participant, consistent with the explicit instruction to record/upload and share the link.

## Checks actually performed

Read `AGENTS.md`, Phase 8 of `ACTION_PLAN.md`, invoice-assignment skill, saved exact workshop/follow-up prompts; line-numbered helper/runbook; `docs/evidence/phase-8/{rehearsal.txt,serve-rehearsal.txt,rehearsal-metadata.json}`; existing `e2e/playwright.config.ts` and `e2e/package.json` to verify runbook command/reporting behavior. Used local `exec_command` through `functions.exec` and messages to parent; no external tools.

Read-only Python `ast.parse` succeeded for the helper. SHA-256 values at review:

- Helper: `0a890dcb25fc47b941a68a9ea4fc74cdbb5990764e6e29570752c40c926010c3`.
- Runbook: `645494f30dcc40e810d7824cbd8ae069618f34d1b1def8165bc1b29f18ec504b`.

Read-only SHA-256 comparison against `docs/evidence/phase-7/tested-input-hashes.json`: **47 entries, zero mismatches**. Socket connection probes to recorded ports **52097** (default run) and **52136** (serve run) both confirmed closed at review time. `git rev-parse HEAD` confirmed the starting revision above; `git status --short` distinguished new helper/runbook/evidence from coordinator changes.

Preserved default output records passing HTTP rehearsal branches and normal shutdown. Serve output records the same passes, three pending UI seeds (`INV-1011`–`INV-1013`), Ctrl+C and shutdown. Metadata records serve exit code 0, three seeds, owned server stopped, and video neither recorded nor uploaded. These are coordinator execution records; this reviewer did not claim to execute those runs.

## Uncertainties and limits

- The helper was inspected and syntax-checked, not rerun by this reviewer. Default/serve/SIGINT execution evidence comes from the coordinator. Closed ports now support cleanup but do not alone prove every possible process/failure schedule.
- No failure-injection, forced-kill, cross-platform process-group or port-allocation stress test was run. The script targets this local Unix environment. A process killed without Python cleanup cannot be guaranteed to run `finally`; the supported runbook stop action is Ctrl+C.
- Plain Python assertions require the documented ordinary `python3` invocation; optimized Python (`-O`/`PYTHONOPTIMIZE`) would disable them. The runbook does not request optimized mode. The helper is demo preparation rather than a CI test gate.
- UI seeds were not manually consumed in a browser during this review. Their pending creation and flow usability follow from source, coordinator evidence and already-reviewed application behavior. Rehearsal checks do not validate every timestamp/schema/race property independently; application tests remain authoritative.
- Installation duration, recording timing, UI readability, recorder output and final hosted playback/access cannot be certified from these files. The runbook instructs dependency installation before filming. Recording/upload and sharing the URL remain participant work, with no fabricated completion or approval.
- This review does not certify forthcoming log-audit/phase-summary documents or final submission completeness. Subsequent helper/runbook changes require a fresh check.
