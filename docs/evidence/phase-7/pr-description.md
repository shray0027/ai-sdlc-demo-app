Managers can approve invoices through USD 50,000 and Directors through USD 500,000. Above USD 500,000, a Manager or Director records a first approval; a different authenticated user with Director role must complete it. Failed attempts return clear reasons and preserve the complete stored invoice and approval history.

## Implementation

- Domain policy uses canonical server-session identity/role, exact positive USD cents, and atomic copy-on-write repository transitions.
- UI displays pending first approval and visible rejection reasons. The Chromium regression verifies Manager rejection at USD 50,000.01 and unchanged persisted state after reload.
- Separate implementation commits: `5cf4d61` (single-approver limits) and `df5fa14` (two distinct approvers); `437dd89` adds browser coverage and meaningful test strengthening. The PR preserves nine independent commits before Phase 7 evidence updates.

## Verification

Final local checks on `0b91c4a`: Release build has zero warnings/errors; 173 .NET tests, 46 JavaScript tests and one Chromium test pass. Strict OpenSpec validation passes. Remote GitHub CI passed on initial head `0b91c4a` and review head `8bc6966`; exact check metadata is preserved in `docs/evidence/phase-7/`.

Mutation: before **100.00%** (97 Killed, 3 Timeout); after **100.00%** (100 Killed, 0 Timeout). Both have 135 generated/100 active/35 ignored and zero survivors/no-coverage. Production, scope, configuration and mutant fingerprints match; no numerical score increase is claimed. Raw reports and precise revision references are in `docs/evidence/phase-6/mutation/`.

## Review findings and participant verdicts

The participant supplied the exact workshop prompt, preserved in `docs/agents/workshop-review-prompt.txt`, and explicitly answered `Reviewed all nine; mark each Fixed` for all nine earlier findings below. An independent review of base `66cb3f9` to head `8bc6966` applied that prompt and reported **no actionable new findings**. See `docs/agents/workshop-review.md` for rule tracing, actual checks and uncertainties. Subsequent changes contain review evidence/tracking only; all 47 tested code/config hashes still match.

| Finding | Agent handling/evidence | Participant verdict |
| --- | --- | --- |
| SPEC-01: distinguish temporary stage-one high-value tests from final stage-two expectations | Clarified/rechecked; `docs/agents/specification-review-dispositions.md` | **Fixed** |
| S1-TR-01: reject JSON amounts before decimal conversion can round them | Exact converter and API regressions; `docs/agents/stage-one-test-review.md` | **Fixed** |
| S1-TR-02: use an independent expected state for successful repository mutation | Complete independent oracle and recheck; same report | **Fixed** |
| S1-TR-03: verify complete stable race responses | Full expected state/detached response assertions and recheck; same report | **Fixed** |
| S1-C-01: preserve numeric form input before browser rounding | Lexical JSON-number transport and 42 focused Node regressions; `docs/agents/stage-one-coordinator-review.md` | **Fixed** |
| ST2-T01: assert populated first-history preservation on actor/amount/terminal denials | Additional Domain/API matrices; `docs/agents/stage-two-test-review.md` | **Fixed** |
| MI-01: non-draft submission explanation | Useful reason/full state across three statuses; `docs/agents/mutation-investigation-result.md` | **Fixed** |
| MI-02: blank rejection-reason explanation | Useful explanation/full unchanged state; same report | **Fixed** |
| MI-03: authenticated identity explanation | Meaningful wording rather than parameter suffix; same report | **Fixed** |

Every actual earlier finding now has the participant's **Fixed** verdict above. No new workshop findings or extra human approvals were invented. The reviewer independently passed 173 .NET, 46 Node and 3 logger tests; inspected matching mutation reports/browser snapshots; did not rerun Playwright or mutation testing. Review is bounded to the named revision and does not prove all faults absent.

## Evidence and remaining submission work

`ASSIGNMENT.md` links specifications, actual human choices/test-review replies, agent prompts/results, interaction logs, browser evidence, and comparable mutation reports. Source images are excluded. The application uses in-memory storage, so restart persistence is outside this demo. The workshop review and all nine participant verdicts are complete. The final submission write-up/video handoff and uploaded 8–10-minute demo remain pending; no merge or full submission completion is claimed.
