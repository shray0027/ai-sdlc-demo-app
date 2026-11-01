# Phase 6 — browser regression and comparable mutation evidence

Status: complete, including explicit participant review. Test/tooling inputs are committed as `437dd899f782a937da38cb58bb517cd40207aeb1`. Production source is byte-identical to the experiment base `54138b503167d1e0962f7522845856a62256d3fb`. This later evidence/write-up commit contains the reports and literal input revision references.

## Actual verification

| Check | Result | Evidence |
| --- | --- | --- |
| Chromium UI rejection/reload | 1 passed; HTTP 409; exact visible reason; all 11 persisted fields unchanged | [Browser report](phase-6/playwright/README.md), [final state](phase-6/playwright/invoice-state.json) |
| Final Release .NET suite | 173 passed, 0 failed/skipped | [Combined output](phase-6/final-checks.txt) |
| Node unit checks | 46 passed, 0 failed | Same combined output |
| Release build | 0 warnings/errors | Same combined output |
| Independent report comparison | Same 135 mutant fingerprints/source text; only the test file changes | [Coordinator comparison](phase-6/coordinator-comparison.json) |
| OpenSpec strict validation | Passed | [Validation](phase-6/openspec-validation.txt) |

The browser test logs in and creates/submits a unique synthetic USD 50,000.01 invoice through the UI, captures an authenticated fresh API state, attempts approval through UI as Manager, checks the exact limit reason and 409, reloads and compares complete persisted state. Page objects use roles/labels; the token stays in the browser. An owned loopback server is closed after each run and port 5000 is not reused. Text/JSON evidence is committed; screenshots, video, traces and HTML diagnostics are not submitted.

The first browser attempt failed due to insufficient login synchronization, not a production change: existing session-bar CSS made Sign out visible before login completion. The fixed test polls actual authenticated username/role. Failed attempt, agent success and coordinator success are preserved separately. See [coordinator review](../agents/phase-six-coordinator-review.md). Real-browser layout/authentication are exercised here; older DOM-stub Node tests are not mislabeled as E2E.

## Mutation comparison as text

**BEFORE: 100.00%** — 97 Killed, 3 Timeout, 0 Survived, 0 NoCoverage, 35 Ignored; 135 generated, 100 active/tested.

**AFTER: 100.00%** — 100 Killed, 0 Timeout, 0 Survived, 0 NoCoverage, 35 Ignored; 135 generated, 100 active/tested.

There is no numerical score increase. Before counted timeouts as detected. After strengthening, all three former timeout explanation mutants are actually killed and their killedBy identifiers map to the stronger assertions. Timing variation is not excluded; the direct AFTER failure mappings establish actual detection without claiming that a timeout itself proved detection.

Fixed: Stryker.NET 4.13.0 local manifest, .NET 8 SDK 8.0.427, Standard, Debug, vstest, concurrency 2, full .NET test project, same Json/ClearText/Html reporters/config/production hashes. Explicit includes are Domain ApprovalService.cs and UsdAmount.cs. Coverage analysis was set off before either measured run because dynamically timestamped MemberData identities made coverage matching unsuitable. Both measured runs select the entire test project with no filters. Rejected setup-config and interrupted coverage attempts are retained separately and are not baseline scores.

The 35 ignored mutants are unchanged: 24 outside the explicit two-file scope and 11 automatically block-covered filters. No survivor exclusions or arbitrary ignored mutations were added. There are no AFTER survivors/no-coverage/timeouts/errors requiring invented equivalence explanations. Default report display thresholds 80/60 and break 0 are not an assignment score target.

[Raw reports, counts, exact commands and hashes](phase-6/mutation/README.md) and [detection mapping](phase-6/mutation/strengthened-mutant-detection.json) preserve the experiment. The measured worktree stayed detached at base 54138b5; AFTER applied only the [preserved test diff](phase-6/mutation/test-strengthening.diff). Those AFTER tests match integration revision 437dd89, not a fictitious new worktree HEAD. The ignored worktree remains at `.tools/mutation-worktree`; temporary binaries/HTML stay local.

## Meaningful strengthening and review

After the untouched 171-test BEFORE run, the read-only child found three explanation assertions worth improving despite zero survivors:

- MI-01: non-draft Submit now checks a useful explanation plus full fresh stored state for PendingApproval/Approved/Rejected (two extra cases).
- MI-02: blank rejection reason now checks a useful explanation and complete unchanged state.
- MI-03: invalid identity checks meaningful wording instead of allowing ArgumentException's parameter-name suffix alone to satisfy `Contains(identity)`.

Focused ApprovalService cases passed 73/73; full strengthened suite passed 173/173. Existing threshold, duplicate Director and full-history cases were retained. [Exact child prompt](../agents/mutation-investigation-prompt.txt) and [result/handling](../agents/mutation-investigation-result.md) distinguish timeout observations, code review and actual test execution. No weakness or score improvement was fabricated.

Coordinator `/root` used the Playwright/invoice-assignment skills and OpenSpec apply tracking. Browser agent `/root/phase_six_browser` used Playwright and invoice-assignment testing guidance. Mutation agent `/root/phase_six_mutation` used invoice-assignment testing guidance and delegated `/root/phase_six_mutation/mutation_test_review` for read-only report/contract review. Actual task/parent/session identifiers, prompts and tool results are preserved in structured logs/transcript exports. No workshop review is implied.

## Participant acknowledgment and tracking

For input commit `437dd89`, the participant explicitly answered **`Reviewed; satisfied with both additions`** to the browser/strengthened-test assessment. This independently closes the participant portions of tasks 4.2 and 4.4; prior stage confirmations were not reused. [Review guide](phase-6/TEST_REVIEW.md), [acknowledgment](phase-6/participant-review.json).

Tasks 4.1–4.5 are checked through exact task-source verification and refreshed CLI progress: [progress](phase-6/task-progress.jsonl), [final apply state](phase-6/openspec-apply-final.json). Overall progress is **23/26**, with final integration, workshop PR review and submission handoff still open. [ASSIGNMENT.md](../../ASSIGNMENT.md) now includes both scores as text, commands/revisions/config/counts and actual human/agent evidence; PR/demo links remain pending.

Next: Phase 7 pull request and exact workshop diff review, with participant verdicts for every finding. No PR URL, workshop prompt, video, remote CI execution or individual human finding verdict is invented.

Console/help text has trailing whitespace normalized for repository checks; raw JSON reports and the applicable unified test diff retain their original bytes.
