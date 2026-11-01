# Phase 6 participant test review

Status: complete. Exact participant answer on 1 November 2026: `Reviewed; satisfied with both additions`. Review input revision: `437dd899f782a937da38cb58bb517cd40207aeb1`. Prior stage-one/stage-two confirmations do not approve these new additions.

## Browser rejection test

[manager-limit.spec.ts](../../../e2e/tests/manager-limit.spec.ts) and its [page object](../../../e2e/pages/invoice-app.page.ts) perform real Manager login, UI creation/submission of a unique USD 50,000.01 invoice, visible exact rejection reason plus HTTP 409, reload, and fresh authenticated API equality for all 11 invoice fields. Locators use accessible roles/labels. The test owns its loopback server and does not reuse port 5000.

The final coordinator browser run passed 1/1; 173 .NET tests and 46 Node checks also passed. [Final combined output](final-checks.txt), [persisted before/after invoice](playwright/invoice-state.json), [server teardown](playwright/coordinator-run.json).

## Strengthened .NET assertions

Review [ApprovalServiceTests.cs](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) and the [exact strengthening diff](mutation/test-strengthening.diff):

- Non-draft submission now checks a useful explanation and complete fresh state for PendingApproval, Approved and Rejected (two extra test cases).
- Blank rejection reason now requires an explanation, with a complete fresh-state check.
- Blank actor identity requires meaningful explanatory wording; a parameter-name suffix containing “identity” alone is insufficient.

These are test-only changes; production source is unchanged. The [independent investigation](../../agents/mutation-investigation-result.md) documents MI-01/02/03 and agent handling. Passing tests do not supply individual participant finding verdicts.

## Actual mutation comparison

| Result | Before | After |
| --- | ---: | ---: |
| Stryker score | 100.00% | 100.00% |
| Killed | 97 | 100 |
| Timeout | 3 | 0 |
| Survived / NoCoverage | 0 / 0 | 0 / 0 |
| Ignored | 35 | 35 |

There is no numerical score increase: Stryker counted timeouts as detected in the before run. After strengthening, all three formerly timed-out explanation mutants are actually killed by the strengthened tests. [Detection mapping](mutation/strengthened-mutant-detection.json) and [raw comparison](mutation/README.md) preserve the evidence. Same production/config/tool bytes, scope and all 135 mutant fingerprints were independently verified.

The participant explicitly confirmed satisfaction with both the browser test and the strengthened .NET tests. This completes the actual participant review needed by OpenSpec tasks 4.2 and 4.4. It does not substitute for the later workshop PR review or assign individual finding verdicts.
