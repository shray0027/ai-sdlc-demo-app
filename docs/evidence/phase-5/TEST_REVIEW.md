# Stage-two generated-test review

Participant review status: pending. Stage-one review completion applies to commit `5cf4d61`; it is not reused as approval of these new tests.

Implementation checks: **171 .NET tests** and **46 Node tests** passed. The independent child inspected new coverage and rechecked its one reported gap. This guide links the current tests for the participant's actual review.

| Focus | Tests/evidence |
| --- | --- |
| Manager or Director first above 500,000 leaves PendingApproval; distinct Director second completes and retains history, including decimal maximum | [ApprovalServiceTests](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs), HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory |
| Same Director twice; same canonical identity after role change; different Manager second; independent full-state equality after each denial | ApprovalServiceTests.HighValue_DuplicateBeforeWrongRoleAndDistinctManagerPreserveState; [ApprovalsControllerTests](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs), SameDirector_RepeatedAndActualReloginTokenCannotCountTwice / SecondApproval_UsesCanonicalSessionIdentityAndRole |
| Real login, re-login/new token, initial null camelCase metadata, pending/final response and fresh GET | ApprovalsControllerTests.HighValue_RealLoginsFirstAndSecondAndFreshGetExposeFullHistory; [InvoicesControllerTests](../../../backend/tests/InvoiceApp.Tests/InvoicesControllerTests.cs) |
| Partial/invalid first metadata, incompatible final metadata, blank reasons, terminal states, unsupported actors and invalid amounts preserve populated first history | Domain and API metadata/history cases, including the ST2-T01 additions recorded in [independent review](../../agents/stage-two-test-review.md) |
| Intentional rejection by either eligible role, including the first approver, preserves first identity/role/time | Domain IntentionalReject_PreservesFirstApprovalIncludingSameActor and API IntentionalRejectionAfterFirst_PreservesHistoryAndTerminalFailures |
| Detached copies/rollback include new fields; concurrent same-user, distinct Directors, Manager/Director ordering, final approval races and approve/reject/delete produce valid serial outcomes | [InMemoryInvoiceRepositoryTests](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs) |
| Actual app rendering of server progress after refresh/fresh context, completed pending-item removal, visible errors and available Manager action | [approval-progress.test.cjs](../../../backend/tests/ui/approval-progress.test.cjs), four DOM/fetch-stub checks; all 42 exact-create transport cases retained |

Only four Domain and two API temporary unavailable-workflow rows were superseded. Lower-value role/limit, precision, authority, snapshot and API-error regressions remain. Full snapshots serialize all current invoice properties independently of production copying and compare fresh stored state.

The Node checks are JavaScript unit checks, not Playwright or a real-browser execution. Mutation testing and the workshop PR review are later work.

OpenSpec task 3.8 requires the participant's actual generated-test review separately. Please inspect these cases and report completion or requested changes. Agent handling of ST2-T01 does not supply a human finding verdict.
