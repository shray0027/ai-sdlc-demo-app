---
name: invoice-assignment
description: Specify, implement, test, or review the invoice approval limits assignment in ai-sdlc-demo-app, preserving staged changes and execution evidence.
---

Read root `AGENTS.md` and the relevant phase of `ACTION_PLAN.md`. Use the mode matching the delegated task; do not implement later phases during context checks.

- **Specification:** Use the actual selected spec-driven tool to generate its artifacts. Map FR-01–FR-05 to acceptance cases; resolve the first high-value approver, role migration, decimal precision, identity, pending representation and concurrency. Agent proposals are not participant approvals.
- **Implementation:** Stage one is single-approver limits with tests. Stage two is the distinct-user/Director-second transition, in a separate task and commit. Keep rules in Domain, authenticated role/identity in API, and presentation in the UI. Validate before mutating live repository references.
- **Testing:** Compare every stored invoice field around rejected attempts, not just status. Exercise threshold equality, adjacent values, repeat identity including Directors, invalid second role, and valid completion. Preserve production revision and matching scope for both mutation runs. Use the available Playwright skill when automating the UI.
- **Review:** Trace each rule through Domain, API, persisted state, UI and tests. Use the workshop's exact prompt when available; label any independent review otherwise. Give findings IDs and evidence. Leave human verdicts pending until the participant supplies them.
- **Context check:** Explain the rules, layers, run/test commands, likely change locations and unresolved decisions from repository context. Make no production edits.

Before delegating, save the exact prompt with its task ID. After work, record actual results and child-agent relationships in the interaction log. Invoke `scripts/interaction_log.py` as documented in `logs/README.md` to preserve available transcript evidence. Do not represent manual notes or fixtures as automatic captures.
