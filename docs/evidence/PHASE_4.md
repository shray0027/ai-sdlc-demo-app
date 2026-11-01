# Phase 4 — single-approver implementation

Status: technical implementation and independent agent verification complete; participant generated-test review remains pending. This evidence is included in the standalone stage-one implementation commit. Resolve its revision with `git log -- docs/evidence/PHASE_4.md` rather than treating the pre-implementation revision as the tested feature commit.

## Resulting behavior

- Manager final approval through USD 50,000 inclusive; Director through USD 500,000 inclusive. Rules run in Domain, with canonical identity/role obtained from the server session.
- Above USD 500,000, this intermediate stage returns a clear two-person-workflow-unavailable 409 with complete unchanged state. No first-approval fields or two-person implementation is included; Phase 5 replaces only the deliberately temporary high-value denial expectations.
- Positive numeric USD cents enforced at creation and Domain approval; trailing-zero representations accepted without rounding. Exact numeric JSON conversion prevents lossy decimal binding. The creation form preserves raw numeric value without JavaScript Number conversion.
- Repositories use shared-lock copy-on-write transitions and detached input/read/commit/result snapshots. Working-copy exceptions commit nothing; unrestricted stale Update is removed. Submit/approve/reject/delete serialize consistently.
- Clear JSON errors for authentication, role, missing invoice, invalid input and approval conflict. Admin retains deletion but has no approval/rejection bypass; legacy/unknown roles are denied.
- demo.approver is Manager; demo.director1 and demo.director2 are distinct Directors. README and UI hints match the implemented stage.

## Verification

Commands run from the app root with the repository-local .NET 8 SDK; Node v24.18.0 used for JavaScript checks:

```sh
.tools/dotnet/dotnet test backend/InvoiceApp.sln
node --test backend/tests/ui/*.test.cjs
.tools/dotnet/dotnet build backend/InvoiceApp.sln --configuration Release
.tools/dotnet/dotnet test backend/InvoiceApp.sln --configuration Release --no-build
```

| Check | Actual result | Evidence |
| --- | --- | --- |
| Debug Domain/API/repository suite | 98 passed, 0 failed, 0 skipped | [implementation output](phase-4/implementation-tests.txt) |
| Initial coordinator Release build/test | Build 0 warnings/errors; 98 passed, 0 failed | [build](phase-4/release-build.txt), [tests](phase-4/release-tests.txt) |
| Exact creation-form submit handler | 42 passed, 0 failed; independent child rechecked | [Node output](phase-4/ui-amount-tests.txt) |
| Final CI-equivalent commands after form correction | Results captured with final production/test hashes | [final output](phase-4/final-checks.txt), [file hashes](phase-4/tested-file-hashes.json) |
| OpenSpec structural validation | Final submitted stage tracking remains valid | [validation](phase-4/openspec-validation.txt) |

The tests compare all current public invoice properties using an independent serialization oracle and fresh stored reads; they do not use production copying to create expected failure state. Real/fake repository copying, retained references and rollback are exercised. Real repository races use separate service instances and assert valid serial outcomes plus full stable responses. API tests include actual configured-user login at both limits, forged actor/role metadata, denied sessions/roles, malformed and unsupported precision inputs, and missing invoices. [Review guide](phase-4/TEST_REVIEW.md) maps the concrete cases to source files.

The JavaScript tests use the actual submit handler in a Node vm with DOM/fetch stubs; they are not Playwright or real browser tests. Browser execution and mutation evidence remain later assignment phases. The local Release checks mirror CI; no GitHub Actions execution is claimed.

## Delegation and review

- Coordinator `/root`: apply instructions, integration/source review, exact browser-input reproduction, Release/CI checks, context/plan/log updates and standalone commit.
- Implementation `/root/stage_one`: scope limited to single-approver tasks, tests, account/docs updates and the exact transport correction. [Initial prompt](../agents/stage-one-implementation-prompt.txt), [transport follow-up](../agents/stage-one-transport-followup-prompt.txt).
- Child `/root/stage_one/test_review`, parent `/root/stage_one`: independent code/generated-test review, actual HTTP precision reproduction, test runs and targeted rechecks. [Exact initial/follow-up prompts](../agents/stage-one-test-review-prompt.txt), [review](../agents/stage-one-test-review.md).

Three child findings were addressed and rechecked: S1-TR-01 JSON-to-decimal rounding, S1-TR-02 successful-copy expected-state oracle, and S1-TR-03 complete racing response assertions. The coordinator additionally reproduced/fixed S1-C-01 browser Number rounding. [Coordinator handling](../agents/stage-one-coordinator-review.md). These are agent handling outcomes, not participant verdicts or the workshop PR review.

Actual skills: invoice-assignment implementation/testing/review modes and OpenSpec apply-change for the coordinator/implementation agent; invoice-assignment review/testing guidance for the child. No Playwright skill or mutation tool was invoked.

## Tracking and human input

OpenSpec apply was ready with 1/26 tasks complete at start. Stage-one delegation and tasks 2.1–2.7 are now checked and verified through refreshed CLI instructions: [progress](phase-4/task-progress.jsonl), [final apply state](phase-4/openspec-apply-final.json). Progress is 9/26; task 2.8 stays open because the participant's actual generated-test review has not arrived. Technical test execution and commit are recorded separately instead of inventing human completion.

The participant said `start`, authorizing stage one after the reviewed specification. Earlier explicit policy choices remain Manager-or-Director high-value first approval and positive USD cents. A concrete test-review guide and asynchronous assessment question were provided after the working tests existed; no response is inferred. Any later participant feedback must identify the reviewed revision and be recorded without rewriting raw capture history.

## Practical limits and next stage

State is in memory and locking is local-process only. Restart/sign in again after role migration. Very large returned values can be displayed approximately by JavaScript number formatting; creation transport and server storage/approval remain exact. Phase 5 is the separate two-distinct-approver implementation. Workshop review prompt, participant findings verdicts, Playwright, mutation evidence, PR publication and demo remain pending.
