# Approval policy decisions — Phase 3

Status: participant choices received on 1 November 2026. Other design choices remain explicitly agent-selected; independent specification review and recheck are complete, without implying participant artifact approval.

| Topic | Observed baseline | Proposed decision | Authority/status |
| --- | --- | --- | --- |
| First approver above 500,000 | No two-person workflow exists | Manager or Director may record the first approval; it does not approve the invoice by itself. A distinct Director must finish it. | Participant selected “Manager or Director (recommended)” |
| Currency and precision | UI formats USD and steps by 0.01; API accepts any positive decimal | Keep USD, accept positive amounts with at most two decimal places; reject rather than round unsupported precision. | Participant selected “USD, at most two decimal places (recommended)” |
| Legacy role migration | demo.approver has Approver role; demo.admin has Admin role | Give demo.approver Manager role; add at least two distinct Director demo accounts; keep Admin as administration only with no approval bypass. | Agent-selected design; independently reviewed, not a participant approval |
| Identity | Login finds an exact configured username and session stores that canonical username | Use canonical authenticated session Username as approver identity; do not accept request-body identity or role. Preserve exact/case-sensitive baseline login. | Agent-selected design grounded in existing authentication |
| Pending first approval | Only final DecidedBy/DecidedAt/DecisionNote exist | Keep status PendingApproval, add first-approver identity/role/time fields; final decision fields remain unset after first approval. | Agent-selected design; independently reviewed |
| Rejected state | Repository returns live invoice objects | Validate before mutation; compare all invoice fields and first-approval metadata on a fresh read. API errors have a clear reason. | Required by FR-05; reviewed copy-on-write design enforces rollback/snapshot isolation |
| Concurrent attempts | Repository calls lock individually | Define a repository-provided atomic invoice mutation boundary; domain callback owns validation and transitions. Serialize competing submit/approve/reject/delete operations affecting that invoice. | Agent-selected design: shared-lock copy-on-write with detached reads/results; independently reviewed |

The five assignment rules remain mandatory regardless of these choices. A rejected approval attempt is distinct from the intentional Reject operation that records a rejection decision.

Exact participant replies: first approval — `Manager or Director (recommended)`; precision — `USD, at most two decimal places (recommended)`. These answers resolve the two material policy questions; they are not a claim of human review of generated specifications or tests.
