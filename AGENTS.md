# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Assignment rules (required behavior, not yet implemented)

Source: `requirements/REQUIREMENTS.md`. Follow `ACTION_PLAN.md` in phases.

- **FR-01:** Manager approval limit is 50,000 inclusive.
- **FR-02:** Director approval limit is 500,000 inclusive.
- **FR-03:** Above 500,000, require two distinct approvers; the second must be a Director.
- **FR-04:** One user must never count as both approvers, including a Director approving twice.
- **FR-05:** Every rejected approval attempt must give a clear reason and leave the entire invoice and its approval records unchanged.

Do not assume Admin bypasses these rules. The current Approver/Admin roles are baseline behavior. Phase 3 decisions are recorded in `docs/decisions/approval-policy.md` and the reviewed draft at `openspec/changes/invoice-approval-limits/`. The participant chose Manager or Director for the high-value first approval and positive USD values with at most two decimal places. Agent-selected design uses canonical authenticated username identity, separate first-approval metadata while PendingApproval, Manager/Director approval roles without an Admin bypass, and atomic copy-on-write repository transitions. These are specification decisions; implementation remains pending.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every API endpoint except login requires sign-in. Destructive actions are Admin only. Baseline approvals allow Approver/Admin; migrate authorization deliberately with the specification. Pass role and identity from the authenticated server-side session, never trust a role supplied in an approval request body.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.
- The in-memory repository returns live references. Validate before mutating any field; a failure before `Update` can still corrupt stored state. Specify atomic transitions for concurrent approval requests.

## Test and review expectations

- Implement single-approver limits and two-approver transitions in separate delegated tasks and commits. Keep the existing language and layers.
- Test inclusive boundaries and adjacent supported monetary values. Negative tests must compare complete state before/after, including first-approval metadata, timestamps and any version fields added. Read the stored invoice again after a rejection.
- Include a Playwright Manager over-limit rejection with a visible reason and unchanged invoice after reload. No baseline browser tests exist yet.
- Preserve comparable before/after mutation reports for approval-rule code; strengthen meaningful assertions based on survivors. No target mutation score was specified.
- Review generated tests and the final diff. Agent review is distinct from participant judgment: the participant supplies each finding's fixed/dismissed/deferred verdict. Obtain the exact workshop review prompt before claiming workshop review.
- Do not invent human review, test execution, hook delivery, mutation scores or demo links. Keep unresolved decisions explicit.

## Skills, delegation, and evidence

- Use `.agents/skills/invoice-assignment/SKILL.md` for this assignment's specification, staged implementation, tests, or review; load only the relevant mode.
- Coordinator owns integration, logs and plan. Role definitions and actual task/session relationships live in `docs/agents/README.md`.
- Phase 2 explicitly authorizes a fresh context-check agent and a test sub-agent. Other responsibilities are assigned as their phases start; avoid concurrent edits to shared files.
- Lifecycle hook configuration: `.codex/hooks.json`. Logging, transcript export, and coverage limits: `logs/README.md`.
- Run `python3 scripts/interaction_log.py sync --session <rollout.jsonl>` for an explicitly selected assignment transcript. Hook delivery is verified independently; file creation does not prove activation.
- Record exact delegated prompts, results, skills actually read, tools, human interventions, timestamps, checks and commit references. Preserve `related_ids` and `parent_task`. Unknown model/session values stay unknown until observed.
- Source images under `requirements/` stay local and ignored; commit the transcribed requirements only.

## Commands

This machine has a repository-local .NET 8 SDK. From the repository root:

```sh
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
```

Setup and initial results: `docs/evidence/BASELINE.md`.

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm ci && npx playwright install chromium && npm test`

## Demo accounts (not real credentials)

- `demo.approver` / `ChangeMe123!` (role Approver)
- `demo.admin` / `ChangeMe123!` (role Admin)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.
