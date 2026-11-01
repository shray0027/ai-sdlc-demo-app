# Final acceptance scenario map

Tested input revision: `0b91c4acc18a641be463b8803b0eee4d78c4d7f3`. Every named OpenSpec scenario has a concrete test below. Method/theory rows were inspected, including inclusive and adjacent USD cents, both first roles, role-change identity and serial race outcomes. Full Release/Node/Chromium checks and strict validation passed in [final-checks.txt](final-checks.txt).

| OpenSpec scenario | Actual test |
| --- | --- |
| Manager below and at inclusive limit | [InclusiveLimits_SetCanonicalActorAndUtcDecision](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Manager above limit in single-approval range | [EveryFailedApproval_LeavesFreshCompleteStateUnchanged](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [RejectedApprovals_ReturnReasonAndPreserveAllPersistedFields](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Director below and at inclusive limit | [InclusiveLimits_SetCanonicalActorAndUtcDecision](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Director above inclusive limit | [HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Manager first and distinct Director second | [HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [HighValue_RealLoginsFirstAndSecondAndFreshGetExposeFullHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Director first and distinct Director second | [HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Distinct Manager cannot complete | [HighValue_DuplicateBeforeWrongRoleAndDistinctManagerPreserveState](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Same Director tries twice | [SameDirector_RepeatedAndActualReloginTokenCannotCountTwice](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Same Director obtains another session | [SameDirector_RepeatedAndActualReloginTokenCannotCountTwice](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Same identity with different role | [SecondApproval_UsesCanonicalSessionIdentityAndRole](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| All failed approval branches preserve persisted state | [EveryFailedApproval_LeavesFreshCompleteStateUnchanged](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [HighValueHistory_OtherFailedBranchesPreservePopulatedFirstFields](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [ActorOrAmountDenials_PreservePopulatedFirstHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Missing invoice remains absent | [MissingInvoice_RemainsAbsent](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [MissingInvoice_Returns404AndRemainsAbsent](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Accepted cents and trailing zeros | [NumericCents_AcceptsWithoutMultiplicationOverflow](../../../backend/tests/InvoiceApp.Tests/UsdAmountTests.cs); [ValidNumericCents_CreationPreservesExactDecimal](../../../backend/tests/InvoiceApp.Tests/InvoicesControllerTests.cs) |
| Unsupported precision rejected at creation | [InvalidCreation_Returns400AndAddsNothing](../../../backend/tests/InvoiceApp.Tests/InvoicesControllerTests.cs) |
| Malformed existing amount rejected in approval | [EveryFailedApproval_LeavesFreshCompleteStateUnchanged](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Client identity and role spoofing has no authority | [ForgedBodyAndHeaders_CannotReplaceAuthenticatedIdentity](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Unauthenticated or unsupported role denied | [MissingOrInvalidSession_Returns401WithoutChanges](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs); [RejectedApprovals_ReturnReasonAndPreserveAllPersistedFields](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| Demo role migration | [Login_MigratesConfiguredAccounts](../../../backend/tests/InvoiceApp.Tests/AuthControllerTests.cs) |
| First approval preserves pending state | [HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Single-approval final decision | [InclusiveLimits_SetCanonicalActorAndUtcDecision](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs) |
| Intentional rejection after first approval | [IntentionalReject_PreservesFirstApprovalIncludingSameActor](../../../backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs); [IntentionalRejectionAfterFirst_PreservesHistoryAndTerminalFailures](../../../backend/tests/InvoiceApp.Tests/ApprovalsControllerTests.cs) |
| UI explains progress | [actual rendering shows server first approval after refresh and fresh app reload](../../../backend/tests/ui/approval-progress.test.cjs) |
| Simultaneous attempts by same Director | [HighValue_ConcurrentFirstApprovalsHaveOnlySerialOutcomes](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs) |
| Competing distinct first and final approvals | [HighValue_ConcurrentFirstApprovalsHaveOnlySerialOutcomes](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs); [HighValue_CompetingFinalDirectorsCannotOverwriteFirstOrFinalHistory](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs) |
| Approval competes with rejection or deletion | [HighValue_ApproveRejectRacePreservesCoherentHistory](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs); [HighValue_ApproveDeleteRaceNeverResurrectsOrChangesResponse](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs) |
| Detached snapshot and exception isolation | [SnapshotContract_AllReferencesDetachedAndExceptionsDiscarded](../../../backend/tests/InvoiceApp.Tests/InMemoryInvoiceRepositoryTests.cs) |
| Manager rejection survives reload | [Manager cannot approve USD 50,000.01; persisted invoice stays completely unchanged after reload](../../../e2e/tests/manager-limit.spec.ts) |

UI progress uses the actual app JavaScript with DOM/fetch stubs; only the Manager rejection/reload case is Chromium E2E. The mutation experiment remains the unchanged Phase 6 experiment rather than a new score run. Application restart persistence, approximate JavaScript formatting of very large returned values, the exact workshop review, participant finding verdicts, remote CI outcome and the uploaded demo are separate limitations/process items; no completion is inferred.
