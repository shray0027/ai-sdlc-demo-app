# Stage-one generated-test review

Participant review status: complete. On 1 November 2026 the participant replied `done` with this review guide active, following the review request for stage-one commit `5cf4d61`. Recorded as test review completed, with no changes requested. Individual finding verdicts were not supplied.

The implementation agent and its independent test-review child passed the full suite: 98 cases, 0 failed/skipped. The coordinator also passed all 98 cases in Release with a zero-warning build. The creation form correction adds 42 dependency-free Node cases; both test groups are linked below. No stage-two, browser or mutation completion is claimed.

| Review focus | Actual tests |
| --- | --- |
| Manager 49,999.99 / 50,000 inclusive and 50,000.01 rejection; Director 499,999.99 / 500,000 inclusive; temporary high-value rejection | [ApprovalServiceTests](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs), including complete fresh-state comparisons for failure rows |
| Positive numeric USD cents, trailing zeros and decimal-range arithmetic | [UsdAmountTests](../../../backend/tests/InvoiceApp.Tests/UsdAmountTests.cs), [InvoicesControllerTests](../../../backend/tests/InvoiceApp.Tests/InvoicesControllerTests.cs) |
| Fractions beyond decimal precision cannot round into approval tiers | InvoicesControllerTests.InvalidCreation_Returns400AndAddsNothing, including 50000.0000000000000000000000001, 500000.000000000000000000000001 and decimal-max-plus-fraction |
| Real login, canonical actor/role at each inclusive limit despite forged body/header data; clear 400/401/403/404/409 failures and unchanged stored state | [ApprovalsControllerTests](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs), [AuthControllerTests](../../../backend/tests/InvoiceApp.Tests/AuthControllerTests.cs) |
| Detached input/read/commit/result copies, callback retention, exception rollback; competing approve/submit/reject/delete requests with full stable-state assertions | [InMemoryInvoiceRepositoryTests](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs), exercised against real/fake snapshot contracts and real shared repository races |
| Exact creation-form transport, syntax normalization, invalid input and safe customer encoding | [invoice-create.test.cjs](../../../backend/tests/ui/invoice-create.test.cjs), testing the actual submit handler under a Node vm DOM/fetch stub |
| Independent all-field snapshot oracle | [InvoiceState](../../../backend/tests/InvoiceApp.Tests/InvoiceState.cs) serializes all public invoice properties, independently of production copying |

The independent [agent review](../../agents/stage-one-test-review.md) found and rechecked three issues: exact-input rounding, successful-copy expected-state oracle, and complete racing response assertions. The coordinator additionally identified and corrected JavaScript Number rounding in the creation transport, and the same reviewer inspected that correction and passed all 42 Node cases. These agent resolutions do not supply participant verdicts. The Node checks are JavaScript unit tests, not a browser/E2E execution.

OpenSpec task 2.8 is now complete. The exact participant reply is `done`; no more detailed assessment or individual finding verdict is inferred. Existing agent reviews and original execution evidence remain preserved.
