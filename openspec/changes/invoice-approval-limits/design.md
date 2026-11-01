# Design

## Context

See proposal.md for motivation and specs/invoice-approval/spec.md for the behavior contract. The .NET 8 Domain project contains lifecycle rules; API controllers forward authenticated session usernames; the singleton in-memory repository returns live objects and locks only individual calls. The seven lifecycle tests use a live-reference fake and reference Domain only. The static UI already has an alert for decision errors, but no first-approval presentation. There are no baseline browser test files.

This design is necessary because the change crosses Domain, API, storage, authentication and presentation, and introduces security-sensitive historical metadata. Participant decisions are limited to Manager-or-Director high-value first eligibility and positive USD values with at most two decimal places. All implementation, migration, error mapping and representation choices below are agent-selected design. Independent artifact review and its narrow recheck are recorded in docs/agents/specification-review*.md; no participant artifact or generated-test approval is inferred.

## Goals / Non-Goals

**Goals:** preserve existing layers and routes; make each accepted lifecycle transition atomic; make failed transitions unable to alter any stored field; provide stable snapshots for reads, results and assertions; support independently reviewable stage commits.

**Non-Goals:** database migration, distributed locking, multi-currency conversion, new identity provider, general workflow engine, bulk approval, partial invoice approval, persistent audit infrastructure, or changing DiscountCalculator. The local demo stays in memory.

## Decisions

### 1. Put policy and validation in Domain

Introduce a clear role representation for Manager/Director and pass a trusted acting username and role to ApprovalService from the server session. Reject blank identity and unsupported roles in Domain as well as the API boundary. Exact, ordinal canonical username equality matches current configured login semantics; a second session or role change does not make a different user. Do not accept acting identity/role from request bodies or sessionStorage. A browser role can guide presentation only.

Use one shared Domain monetary validator for creation and approval: amount must be greater than zero and numerically divisible by 0.01m with no remainder. Decimal remainder avoids multiplying very large decimals by 100; do not check encoded decimal scale, since 50000.000 is numerically valid cents. Creation invokes the validator before Add; Approve validates stored amounts inside the atomic transition, protecting malformed injected/preexisting invoices. Never assign a rounded value. Controller checks alone were rejected because direct Domain calls and injected invoices could bypass them.

For Approve, validate acting identity/role, existence, pending lifecycle and amount before deciding a branch. Within USD 500,000 use inclusive Manager 50,000 / Director 500,000 final limits. Above 500,000 use a distinct high-value branch: either Manager or Director can record a first approval; if a first approval exists, check identity inequality and Director role before completing. Validate inconsistent first metadata as an invalid lifecycle conflict rather than clearing or overwriting it. Use explicit clear messages; duplicate checks must catch a Director approving twice.

### 2. Store first progress separately from final decisions

Add nullable FirstApprovedBy, FirstApprovedRole, FirstApprovedAt to Invoice; existing JSON camelCase conventions produce firstApprovedBy, firstApprovedRole and firstApprovedAt. First fields begin null and are populated together once. First approval retains PendingApproval and null DecidedBy, DecidedAt and DecisionNote. Second approval sets Approved plus final deciding identity/time and retains first fields. Single-approver approvals leave first fields null.

Intentional Reject remains a distinct Domain operation allowed to Manager/Director with a nonblank reason, including after a first approval. It records Rejected and final rejecting identity/time/note, preserving first metadata. The first approver is allowed to intentionally reject; the distinctness rule concerns counting as both approvers, not the Reject operation. Rejecting terminal states remains forbidden. A new enum status was considered but rejected because nullable progress fields keep the existing four-state lifecycle and route/status consumers compatible.

### 3. Use copy-on-write under the existing singleton repository lock

Extend IInvoiceRepository with an atomic mutation operation, conceptually `Mutate(id, Action<Invoice> transition) -> Invoice`. The repository finds the current record under its existing single private lock; missing records throw before invoking the callback. It clones every field into a detached working invoice, invokes the Domain callback synchronously under the lock, then replaces the stored record with another detached copy only if the callback returns successfully. Return another detached snapshot. Exceptions discard the working copy entirely, even if the callback mutated it before throwing. Cloning on commit prevents an accidentally retained callback reference from mutating storage later.

Keep the lock repository-owned so all scoped ApprovalService instances share it. Use one global lock for this small in-memory demo rather than per-invoice lock dictionaries, optimistic versions, or a new persistence layer. No I/O, await or callback reentry into the repository is allowed inside the synchronous Domain callback. Capture UTC decision time only for a successful branch. Do not add a version field unless implementation demonstrates a need; if added, it becomes part of every snapshot/assertion and changes only on success.

All writes use the same private lock. Submit, Approve and intentional Reject use Mutate; Remove performs deletion under that same lock; Add clones incoming values before storing and returns a detached copy. Retire the unrestricted Update path and migrate its callers, because applying stale read snapshots could overwrite accepted decisions or resurrect removed invoices. GetAll/GetById/GetByIds clone under the lock; no stored references escape. Seed records are owned only by the repository. Copy logic must include every current and newly added field. FakeInvoiceRepository must implement the same behavioral contract, including exception isolation and detached snapshots.

This gives a linearization point at committed replacement/deletion. Two requests from one Director can produce one first approval and one duplicate conflict, never a final approval. Concurrent distinct Directors can validly finish in a serial order, even if clients both thought they were first. Manager-versus-Director first races can yield either one pending first or a completed invoice according to actual ordering and second-role authority. No losing final request overwrites the winner. Approval/delete cannot resurrect a deleted record.

Rejected-operation immutability in concurrency tests means the failing transition commits nothing; another successful operation may still change the global invoice. Use controlled barriers before repository calls and assert valid serial outcomes rather than asserting a globally unchanged invoice across unrelated successful writes.

### 4. Migrate roles and preserve trusted boundary errors

Agent-selected account migration: demo.approver becomes Manager; add demo.director1 and demo.director2 as separate Director demo accounts; demo.admin remains Admin. Keep demo passwords as explicitly non-production fixtures. RequireRole on approval/rejection admits Manager/Director only; Admin still has deletion authority and ordinary authenticated read/create/submit access, with no approval or rejection bypass. Legacy Approver and unknown roles are denied.

SessionAuthMiddleware must return clear JSON `{ error }` with 401 for missing/invalid tokens, and RequireRole must return clear JSON `{ error }` with 403 for role denial. This closes the baseline bare-status gap before Domain is reached. ApiControllerBase retains 404 for missing invoice and 409 for lifecycle/rule conflicts, with 400 for invalid amount/identity. Map unsupported roles consistently to 403 at the API boundary, retaining Domain enforcement for direct calls. Malformed request model binding also needs a clear error response; don't rely on a generic empty status. API tests verify nonempty reasons at each boundary and verify role/identity cannot be supplied by the caller.

### 5. Present server results and verify complete stored state

Keep the Manager Approve action available in the above-Manager/below-Director range so the required UI rejection can be exercised. Render first approval progress and “awaiting a distinct Director” from server-returned metadata, including after reload. Display decision errors through the existing alert. Account role hints do not enforce policy.

Create reusable independent complete-state snapshots for negative tests, including all original fields, first metadata, note, time and any future version/record fields. Take snapshots before attempts and re-read storage afterward; comparing the same reference twice is invalid. Unit cases map to named spec scenarios. Add API integration coverage using the existing .NET test tooling plus an API project reference and compatible ASP.NET test host, and real InMemoryInvoiceRepository concurrency tests with multiple service instances. Verify exception discard even after a callback mutates its working copy, and mutation of read/Add/Mutate results cannot affect storage.

Playwright uses page objects and role/label locators; create a unique USD 50,000.01 invoice, capture its complete API snapshot, sign in as Manager, submit the attempted approval through the UI, see a limit reason, reload, and compare the complete fresh API result plus visible pending status. The browser test does not substitute for full API/domain branch coverage.

### 6. Separate stage implementation and evidence

Stage one migrates trusted roles, validates cents, installs atomic repository semantics and final role limits, with tests. Above USD 500,000 approval fails closed with a clear two-person-workflow-unavailable conflict and no mutation; it does not record first metadata until stage two. This intermediate commit intentionally implements only the first stage, and must not claim the final FR-03 acceptance contract is already met.

Stage two separately adds first metadata, two-person transitions, progress UI, Director-duplicate/new-session tests and concurrency cases. Replace the explicitly temporary stage-one above-500,000 unavailable-workflow denial tests with the reviewed first/second transition cases; retain and rerun all lower-value role/limit, amount, identity and immutable-failure regressions. Separate delegated prompts and commits are required. Unit/API results, generated-test participant review, Playwright and mutation runs remain future evidence. Compare initial versus strengthened tests using unchanged production code, identical approval-rule scope/configuration/tool version and recorded commit references; document differences if unavoidable. No target mutation score is invented. Workshop review requires the exact workshop prompt and participant verdicts for every finding.

## Risks / Trade-offs

- Global lock serializes unrelated invoice writes → bounded critical sections with in-memory copying only are appropriate for the small demo; no distributed guarantees claimed.
- A missed copied field breaks snapshot isolation or complete-state assertions → centralize cloning, explicitly cover every field and populate sentinel metadata in negative tests.
- Username identifies configured accounts rather than real people → use unique canonical demo usernames and preserve exact login; a future rename/identity-provider migration needs a separate stable-ID design.
- New Director accounts and loss of Admin approval authority change demo compatibility → update account hints/docs and test Admin/legacy denials; label migration as agent-selected.
- JSON/UI may make first approval look final → render pending progress and keep final fields null until completion.
- Large decimals may exceed JavaScript exact cents → server decimal validation is authoritative; boundary tests use exactly representable JSON numeric examples; large-value UI transport is a demo limitation to disclose rather than silently rounding server values.
- Test fake can hide real lock bugs → test real repository snapshots and races with separate service instances, not just the fake.

## Migration Plan

Commit the reviewed planning artifacts first. Implement and validate stage one in its own commit, then stage two in a separate delegated task/commit. In-memory restart reseeds records; there is no durable data migration. Initialize new nullable metadata as null for existing seeds and preserve existing completed decision fields. Restart and sign in again after account migration because existing sessions retain their stored baseline roles. Rollback means reverting feature commits and restarting the demo, with expected loss of in-memory data. No deployment or rollback has been executed.

## Open Questions

No policy question blocks this proposed design. Independent artifact review, participant generated-test review, the exact workshop review prompt, and participant findings verdicts remain pending process inputs; they are not represented as completed approvals.
