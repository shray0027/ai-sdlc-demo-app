# Phase 5 — two distinct approvers

Status: complete, including explicit participant satisfaction with stage-two tests. The implementation revision is `df5fa1427501266bbcd7e91cfe35279cbaed458b`. The preceding stage-one implementation is commit `5cf4d61`, with participant confirmation recorded in `e869ce4`. This evidence is committed alongside the separate stage-two implementation; source/test hashes identify the exact checked files.

## Behavior and integration

- Above USD 500,000, either Manager or Director records the first approval. Status remains PendingApproval; first identity, historical role and UTC time are populated together; final decision fields remain null.
- Completion requires a different canonical authenticated username with Director role. The same Director cannot count twice using another login/token; a role change does not change user identity. A distinct Manager cannot complete approval.
- All validation happens before successful state changes, within the existing shared-lock copy-on-write boundary. Rejected attempts commit no invoice/history changes. Invalid/partial first metadata or incompatible final metadata on a pending high-value invoice fails closed.
- Completion and intentional rejection preserve first history. The first approver may intentionally reject; this is not a second approval.
- Nullable firstApprovedBy/firstApprovedRole/firstApprovedAt are exposed in invoice JSON, initially null. Existing memberwise copying safely includes these scalar/immutable fields; tests now populate and mutate all new fields around snapshot/rollback boundaries.
- UI progress comes from returned server metadata, persists after refresh/fresh app context, and identifies the first approver and distinct-Director requirement. Visible errors and the Manager over-limit Approve action remain. README/index hints describe the final workflow.
- Inclusive lower-value limits, exact USD cents, exact creation transport, authenticated role/identity and administrative boundaries remain unchanged. Only four temporary Domain and two API high-value-unavailable rows were replaced by final workflow cases.

## Actual checks

From the application root using repository-local .NET 8 and Node 24.18.0:

```sh
.tools/dotnet/dotnet test backend/InvoiceApp.sln
node --test backend/tests/ui/*.test.cjs
.tools/dotnet/dotnet build backend/InvoiceApp.sln --configuration Release
.tools/dotnet/dotnet test backend/InvoiceApp.sln --configuration Release --no-build
```

| Check | Result | Evidence |
| --- | --- | --- |
| Implementation full .NET suite | 171 passed, 0 failed/skipped | [Debug output](phase-5/implementation-tests.txt) |
| Implementation full Node suite | 46 passed, 0 failed | [Node output](phase-5/implementation-node-tests.txt) |
| Final coordinator CI-equivalent checks | 46 Node and 171 Release .NET passed; build 0 warnings/errors | [Final output](phase-5/final-checks.txt), [tested-file hashes](phase-5/tested-file-hashes.json) |
| Independent child targeted checks | 56 .NET / 4 rendering cases; affected recheck 27 .NET passed | [Child review](../agents/stage-two-test-review.md) |
| OpenSpec strict validation | Passed for updated stage tracking | [Validation](phase-5/openspec-validation.txt) |

Node checks retain all 42 exact-create cases and add four actual-app rendering/error checks under DOM/fetch stubs. They are not a real-browser/Playwright execution. Local Release commands match CI; no remote GitHub Actions run is claimed. No mutation score is claimed.

## Review and delegation

Coordinator `/root` delegated a new, separate stage-two prompt to `/root/stage_two`, which delegated its independent read-only review to `/root/stage_two/test_review`. [Implementation prompt](../agents/stage-two-implementation-prompt.txt), [child prompts/follow-ups](../agents/stage-two-test-review-prompt.txt), [child report](../agents/stage-two-test-review.md), and [coordinator review](../agents/stage-two-coordinator-review.md) preserve the work and limits. Actual session and parent identifiers are recorded in interaction metadata and transcript exports.

The child reported ST2-T01: denial matrices needed populated first-history assertions and API terminal-history variants. Those tests, explicit initially-null API fields and malformed-final-field cases were added, then independently rechecked. Technical resolution is recorded; no participant verdict is inferred. The coordinator's minor progress-layout grouping was followed by the final full Node/Release checks.

Actual skills were invoice-assignment implementation/testing/review guidance and OpenSpec apply-change for the coordinator/implementation agent; the child used invoice-assignment review/testing guidance. No browser skill or mutation tool was invoked.

## Tracking and participant review

Apply started ready at 10/26 complete. Delegation and tasks 3.2–3.7 are now checked through exact source-line verification and refreshed CLI instructions: [progress](phase-5/task-progress.jsonl), [final apply state](phase-5/openspec-apply-final.json). Progress is **17/26**. Task 3.8 remains unchecked only for the participant's actual review of the new tests; technical implementation/review/execution and the separate commit are recorded independently.

[Stage-two test-review guide](phase-5/TEST_REVIEW.md) links the concrete new tests and retained regressions. An assessment question was sent after those tests existed. No answer is inferred; the earlier `done` applies to stage-one review only.

Next is Phase 6: Playwright rejection coverage and comparable before/after mutation evidence. Workshop prompt, individual participant finding verdicts, PR publication, write-up and demo remain pending. Persistence remains local in-memory; very large returned values may be displayed approximately by JavaScript formatting, while creation and stored approval values remain exact decimals.

## Participant test-review confirmation

On 1 November 2026 the participant explicitly answered `Reviewed; satisfied with the tests` to the stage-two review question. This completes task 3.8 for commit `df5fa14`, with no changes requested. The earlier pending descriptions above record the implementation-time state; current progress is 18/26, captured in `phase-5/participant-review-progress.json`. No new test execution or separate human finding verdict is inferred.
