# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Assignment rules and stage status

Source: `requirements/REQUIREMENTS.md`. Follow `ACTION_PLAN.md` in phases.

- **FR-01:** Manager approval limit is 50,000 inclusive.
- **FR-02:** Director approval limit is 500,000 inclusive.
- **FR-03:** Above 500,000, require two distinct approvers; the second must be a Director.
- **FR-04:** One user must never count as both approvers, including a Director approving twice.
- **FR-05:** Every rejected approval attempt must give a clear reason and leave the entire invoice and its approval records unchanged.

Both implementation stages are now present: inclusive single-approver limits plus first/second transitions above USD 500,000. A Manager or Director can give the first high-value approval while PendingApproval; only a distinct canonical username with Director role can complete it. Duplicate identity checks apply across sessions and role changes. FirstApprovedBy/FirstApprovedRole/FirstApprovedAt are nullable, populated together, preserved after completion/intentional rejection, and included in complete-state assertions. Invalid first metadata or incompatible final metadata on a pending high-value invoice fails closed. Admin has no approval bypass; baseline Approver has migrated to Manager.

Participant choices and reviewed design remain in `docs/decisions/approval-policy.md` and `openspec/changes/invoice-approval-limits/`. Canonical server-session identity/role, positive numeric USD cents, exact creation transport and atomic copy-on-write transitions remain authoritative. Stage-one participant test review is complete for `5cf4d61`; stage-two participant test review is complete for `df5fa14` (explicit answer: `Reviewed; satisfied with the tests`). Phase 6 now has a passing real-browser Manager rejection/reload test and comparable Stryker 4.13.0 reports: 100.00% before (97 Killed/3 Timeout) and 100.00% after (100 Killed/0 Timeout), fixed scope/production/config. Participant confirmed satisfaction with both additions for 437dd89. PR #1 is open at https://github.com/shray0027/ai-sdlc-demo-app/pull/1. Phase 7 final local checks and all 27 scenario mappings are complete (task 5.1); the exact workshop prompt was applied by independent `/root/workshop_review` to `66cb3f9..8bc6966`, with no actionable new findings. All nine earlier findings are explicitly participant-confirmed Fixed. Tasks 5.1–5.3 are complete; final demo/submission remains pending. See `docs/evidence/phase-7/FINDINGS.md`.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every API endpoint except login requires sign-in. Destructive actions are Admin only. Approval and intentional rejection allow Manager/Director only; legacy Approver, Admin and unknown roles cannot approve/reject. Pass role and identity from the authenticated server-side session, never trust a role supplied in an approval request body.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.
- Repositories return detached snapshots and use a shared-lock copy-on-write `Mutate` callback for lifecycle changes. Exceptions discard working-copy changes; no unrestricted `Update` exists. Policy stays in Domain; callbacks must not reenter the repository. Current invoice fields are scalars or immutable strings; future mutable reference fields require deep-copy handling.

## Test and review expectations

- Implement single-approver limits and two-approver transitions in separate delegated tasks and commits. Keep the existing language and layers.
- Test inclusive boundaries and adjacent supported monetary values. Negative tests must compare complete state before/after, including first-approval metadata, timestamps and any version fields added. Read the stored invoice again after a rejection.
- Include a Playwright Manager over-limit rejection with a visible reason and unchanged invoice after reload. The browser regression is under `e2e/tests/manager-limit.spec.ts`; its complete-state before/after evidence is under `docs/evidence/phase-6/playwright/`.
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
- Run focused form-transport JavaScript tests: `node --test backend/tests/ui/*.test.cjs` (Node 24 used for validation). These are not browser/E2E tests.
- Run end-to-end tests: `cd e2e && npm ci && npx playwright install chromium && npm test`

## Demo accounts (not real credentials)

- `demo.approver` / `ChangeMe123!` (role Manager)
- `demo.director1` and `demo.director2` / `ChangeMe123!` (distinct Director accounts)
- `demo.admin` / `ChangeMe123!` (role Admin)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.

Mutation reproduction uses the pinned local tool manifest and `stryker-config.json`; exact isolated-worktree commands and scope/counts are in `docs/evidence/phase-6/mutation/README.md`. Production code was unchanged during Phase 6.
