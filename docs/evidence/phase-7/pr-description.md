Managers can approve invoices through USD 50,000 and Directors through USD 500,000. Above USD 500,000, a Manager or Director records a first approval; a different authenticated user with Director role must complete it. Failed attempts return clear reasons and preserve the complete stored invoice and approval history.

## Implementation

- Domain policy uses canonical server-session identity/role, exact positive USD cents, and atomic copy-on-write repository transitions.
- UI displays pending first approval and visible rejection reasons. The Chromium regression verifies Manager rejection at USD 50,000.01 and unchanged persisted state after reload.
- Separate implementation commits: `5cf4d61` (single-approver limits) and `df5fa14` (two distinct approvers); `437dd89` adds browser coverage and meaningful test strengthening. The PR preserves nine independent commits before Phase 7 evidence updates.

## Verification

Final local checks on `0b91c4a`: Release build has zero warnings/errors; 173 .NET tests, 46 JavaScript tests and one Chromium test pass. Strict OpenSpec validation passes. Remote GitHub CI passed on this initial head; check metadata is preserved in `docs/evidence/phase-7/ci-initial-head.json`.

Mutation: before **100.00%** (97 Killed, 3 Timeout); after **100.00%** (100 Killed, 0 Timeout). Both have 135 generated/100 active/35 ignored and zero survivors/no-coverage. Production, scope, configuration and mutant fingerprints match; no numerical score increase is claimed. Raw reports and precise revision references are in `docs/evidence/phase-6/mutation/`.

## Review findings and participant verdicts

Draft: the exact workshop review prompt has been requested and has not yet been supplied. No workshop review or participant finding verdict is claimed. Earlier test-satisfaction replies do not stand in for finding dispositions.

| Finding | Agent handling/evidence | Participant verdict |
| --- | --- | --- |
| SPEC-01: distinguish temporary stage-one high-value tests from final stage-two expectations | Clarified/rechecked; `docs/agents/specification-review-dispositions.md` | Pending |
| S1-TR-01: reject JSON amounts before decimal conversion can round them | Exact converter and API regressions; `docs/agents/stage-one-test-review.md` | Pending |
| S1-TR-02: use an independent expected state for successful repository mutation | Complete independent oracle and recheck; same report | Pending |
| S1-TR-03: verify complete stable race responses | Full expected state/detached response assertions and recheck; same report | Pending |
| S1-C-01: preserve numeric form input before browser rounding | Lexical JSON-number transport and 42 focused Node regressions; `docs/agents/stage-one-coordinator-review.md` | Pending |
| ST2-T01: assert populated first-history preservation on actor/amount/terminal denials | Additional Domain/API matrices; `docs/agents/stage-two-test-review.md` | Pending |
| MI-01: non-draft submission explanation | Useful reason/full state across three statuses; `docs/agents/mutation-investigation-result.md` | Pending |
| MI-02: blank rejection-reason explanation | Useful explanation/full unchanged state; same report | Pending |
| MI-03: authenticated identity explanation | Meaningful wording rather than parameter suffix; same report | Pending |

All newly reported workshop findings will be added with participant **Fixed**, **Dismissed with reason**, or **Deferred with reason** verdicts before review completion.

## Evidence and remaining submission work

`ASSIGNMENT.md` links specifications, actual human choices/test-review replies, agent prompts/results, interaction logs, browser evidence, and comparable mutation reports. Source images are excluded. The application uses in-memory storage, so restart persistence is outside this demo. The exact workshop review, participant finding dispositions, and uploaded 8–10-minute demo remain pending.
