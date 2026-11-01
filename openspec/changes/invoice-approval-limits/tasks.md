# Tasks

Implementation and future verification boxes remain unchecked. A planning-review box is checked only when linked review and validation evidence exist; it is not implementation or participant test-review evidence. Use the named scenarios in specs/invoice-approval/spec.md as acceptance references.

## 1. Planning review and stage-one delegation

- [x] 1.1 Independently review proposal/spec/design/tasks against FR-01–FR-05, record findings and participant decisions separately, and verify strict OpenSpec validation passes with any resulting revisions. Evidence: `docs/agents/specification-review.md`, `specification-review-dispositions.md`, `specification-review-recheck.md`, `docs/decisions/approval-policy.md`, and `docs/evidence/openspec/18-validation-after-review.txt`.
- [ ] 1.2 Save and delegate this stage-one prompt with task/parent IDs: “Implement only single-approver limits from invoice-approval-limits design: migrate trusted Manager/Director roles, numeric positive USD cents, detached copy-on-write repository semantics, inclusive Manager 50,000 and Director 500,000, clear API errors and full-state immutable failures. Above 500,000 fail closed with a clear unavailable-two-person-workflow conflict. Do not implement first-approval metadata or two-person completion. Add/review stage-one unit/API/repository tests, run regressions, update relevant docs and return a separate stage-one commit.” Verify the actual delegation prompt/result and commit are recorded in interaction evidence.

## 2. Stage one — atomic single-approver limits and tests

- [ ] 2.1 Replace live-reference reads/Update with a shared-lock copy-on-write mutation boundary in IInvoiceRepository, InMemoryInvoiceRepository and FakeInvoiceRepository; route Submit/Approve/Reject and Remove through the same lock contract. Verify repository tests cover detached GetAll/GetById/GetByIds/Add/Mutate results, retained callback references, and exceptions after working-copy mutation committing nothing.
- [ ] 2.2 Add shared Domain positive numeric-cents validation and invoke it before creation storage and inside approval transitions; verify 0.01, 50000.000 and decimal-range values are handled without overflow, while zero/negative/50000.001/500000.001 values fail without rounding or state changes, including injected invoices.
- [ ] 2.3 Pass canonical authenticated username and role into Domain, enforce Manager/Director authority and inclusive final limits, and fail closed above 500,000 in this stage. Verify FR-01 “Manager below and at inclusive limit” / “Manager above limit in single-approval range”, FR-02 “Director below and at inclusive limit”, blank identity and temporary high-value immutable rejection tests.
- [ ] 2.4 Migrate demo.approver to Manager, add demo.director1/demo.director2, retain Admin administration only and deny legacy/unknown approval roles. Update account/behavior docs and verify login roles, Admin deletion and denial of Admin/legacy/unknown approval and rejection authority through API tests.
- [ ] 2.5 Return nonempty JSON error reasons from session middleware, role filters and controller/domain error mapping; verify API tests for 401/403/404/400/409 and “Client identity and role spoofing has no authority”, using server-session identity despite payload/browser metadata changes.
- [ ] 2.6 Add independent complete-state snapshot assertions and fresh stored reads for every stage-one failed approval path, including Draft/Approved/Rejected, missing invoice, invalid amount/identity, role and limit failures. Verify FR-05 “All failed approval branches preserve persisted state” and “Missing invoice remains absent” with populated sentinel metadata, including creation and decision times/notes.
- [ ] 2.7 Add real repository race tests using separate ApprovalService instances for competing single final decisions, submit/approve, approve/reject and approve/delete; verify valid serial outcomes, no mixed final metadata, no resurrection and stable detached responses.
- [ ] 2.8 Review generated stage-one tests as an agent and obtain the participant's actual test-review result separately; run dotnet test backend/InvoiceApp.sln with the local .NET 8 environment, record actual commands/results and create the standalone stage-one commit explaining inclusive limits and atomic immutable failures. Verify no two-person transition is included in that commit.

## 3. Stage-two delegation and two-person implementation

- [ ] 3.1 After the passing stage-one commit, save and delegate this separate prompt with task/parent IDs: “Implement only the two-person extension on the committed single-approver limits, following invoice-approval-limits spec/design. Add nullable FirstApprovedBy/FirstApprovedRole/FirstApprovedAt; above 500,000 accept Manager or Director first while pending, then a distinct Director second. Reject duplicate canonical identity even across sessions or roles, preserve complete state on every failure and preserve first metadata after completion or intentional rejection. Add unit/API/concurrency tests and UI pending progress, rerun stage-one regressions, update docs and return a separate stage-two commit.” Verify the actual delegation/result is captured independently of stage one.
- [ ] 3.2 Add the three first-approval fields and all-or-none invariant to detached copying, seeds, API JSON and complete-state assertions; implement first and final branches only inside Domain callbacks. Verify “First approval preserves pending state”, “Single-approval final decision”, and retained first fields after valid completion.
- [ ] 3.3 Replace only stage one’s temporary above-500,000 “two-person workflow unavailable” denial tests with first/second-approval expectations; retain every lower-value role/limit, amount, identity and full-state regression. Add FR-02 above-limit and FR-03 first/second tests: “Director above inclusive limit”, “Manager first and distinct Director second”, “Director first and distinct Director second”, and “Distinct Manager cannot complete”. Verify USD 500,000.00 still finishes in one valid Director decision and USD 500,000.01 never does.
- [ ] 3.4 Add FR-04 tests for “Same Director tries twice”, “Same Director obtains another session” (API re-login/new token), and “Same identity with different role”; verify clear reasons and fresh full-state equality preserving first identity/role/time and null final fields.
- [ ] 3.5 Cover intentional rejection after first approval by Manager/Director with nonblank reason, blank reason and terminal-state failures, plus partial/malformed first metadata failing closed. Verify “Intentional rejection after first approval” preserves first metadata and every failed branch uses complete snapshots.
- [ ] 3.6 Add high-value real-repository races for same Director first requests, distinct Director requests, Manager/Director ordering, competing final approvers and approve/reject/delete; verify “Simultaneous attempts by same Director”, “Competing distinct first and final approvals” and “Approval competes with rejection or deletion” as valid serial outcomes without overwritten history.
- [ ] 3.7 Render pending first-approval identity/progress and distinct-Director requirement from server JSON, retain visible decision errors and Manager over-limit Approve action. Update behavior docs and verify “UI explains progress” across refresh/reload without using browser role as authorization.
- [ ] 3.8 Agent-review generated stage-two tests, obtain actual participant test-review feedback, rerun the full unit/API/repository suite with retained stage-one regressions and the explicitly superseding high-value transition tests from 3.3, record results and create the separate stage-two commit explaining two distinct approvers. Verify both implementation commit references are retained.

## 4. Browser and mutation evidence

- [ ] 4.1 Use the Playwright skill and page objects with role/label locators to implement “Manager rejection survives reload”: create unique USD 50,000.01 invoice, submit it, capture full state, attempt Manager approval through the UI, assert visible limit reason, reload and compare complete fresh API state plus visible PendingApproval. Run the test and save actual output/trace and production revision.
- [ ] 4.2 Review the browser test with the participant and record the actual judgment, update test instructions and verify the documented e2e command runs using the repository local SDK environment and isolated test data rather than relying on seed state.
- [ ] 4.3 Set up the assignment-pinned Stryker 4.13.0 for .NET 8 and a focused approval-rule mutation scope; run the initial feature tests and preserve before-strengthening report, command, tool/runtime versions, production/test revisions, scope, counts and actual score as text. Verify unit baseline passes before mutation execution.
- [ ] 4.4 Inspect surviving mutants and add meaningful assertions/cases addressing observed gaps, including threshold operators, duplicate Director logic and full-state metadata; record agent reasoning and participant review of generated improvements, and verify targeted tests fail against the relevant faulty behavior where feasible.
- [ ] 4.5 Rerun mutation testing with unchanged production revision and matching scope/configuration/tool version; preserve after-strengthening report/counts/score and explain survivors or unavoidable comparison differences. Run unit/API/repository and Playwright regression checks and commit test/evidence improvements with actual command/results and revision references. Do not invent a target score.

## 5. Final integration and review handoff

- [ ] 5.1 Verify all named scenarios below are linked to actual tests/evidence, run strict OpenSpec validation and final regression commands against the submitted code, and record unresolved limitations without claiming unexecuted checks.
- [ ] 5.2 Obtain the exact workshop review prompt, run the independent diff review, assign finding IDs and obtain the participant's fixed/dismissed/deferred verdict with reasons for every finding; verify fixes with affected tests and preserve evidence. Keep human review pending until supplied.
- [ ] 5.3 Verify the PR/write-up evidence links the separate implementation commits, participant choices versus agent-selected design, generated-test judgments, UI result, matching mutation reports and actual review findings/verdicts. Deliver artifact links and remaining process requirements to the coordinator without fabricating PR/demo/human-review completion.

## Acceptance-to-test map

| Requirement | Stable scenario names | Planned tests/tasks |
| --- | --- | --- |
| FR-01 | Manager below and at inclusive limit; Manager above limit in single-approval range | Domain theory + API full-state errors (2.3, 2.6); browser reload (4.1) |
| FR-02 | Director below and at inclusive limit; Director above inclusive limit | Domain boundary theory (2.3, 3.3) |
| FR-03 | Manager first and distinct Director second; Director first and distinct Director second; Distinct Manager cannot complete | Domain transition/immutable failure + API (3.2–3.3) |
| FR-04 | Same Director tries twice; Same Director obtains another session; Same identity with different role | Domain identity tests + API re-login + concurrency (3.4, 3.6) |
| FR-05 | All failed approval branches preserve persisted state; Missing invoice remains absent | Every negative case uses independent snapshot/fresh read (2.6, 3.4–3.6) |
| Amount validity | Accepted cents and trailing zeros; Unsupported precision rejected at creation; Malformed existing amount rejected in approval | Domain + creation/approval API (2.2) |
| Trust/migration | Client identity and role spoofing has no authority; Unauthenticated or unsupported role denied; Demo role migration | API/auth tests and direct Domain role denial (2.3–2.5) |
| Progress/history | First approval preserves pending state; Single-approval final decision; Intentional rejection after first approval; UI explains progress | Domain/API + browser progress checks (3.2, 3.5, 3.7) |
| Concurrency/snapshots | Simultaneous attempts by same Director; Competing distinct first and final approvals; Approval competes with rejection or deletion; Detached snapshot and exception isolation | Real repository multi-service races + copy/throw tests (2.1, 2.7, 3.6) |
| API/UI errors | Manager rejection survives reload | Status/JSON error matrix + Playwright (2.5, 4.1) |

## Workflow follow-up

- Coordinator retains ownership of plan/log updates and final publication; keep transcript and task/parent references with actual skill/tool usage.
- Archive only after implementation, checks and required reviews are actually complete; archive is outside this planning task.
