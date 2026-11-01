# Proposal

## Why

The current approval service lets any baseline Approver or Admin finalize any pending invoice. The assignment requires inclusive role limits, two distinct people for high-value invoices, and failed attempts that cannot corrupt stored state.

## What Changes

- Enforce Manager final approval through USD 50,000 and Director final approval through USD 500,000, inclusive.
- Above USD 500,000 allow a Manager or Director to record the first approval as an exception to final-approval limits; only a distinct Director can finish.
- Keep the invoice pending after first approval and expose first approver identity, role and time alongside final decision fields.
- Reject invalid amounts, identities, roles, lifecycle states and duplicate approvers with clear reasons and complete invoice immutability; make concurrent decisions atomic.
- **BREAKING:** replace baseline Approver authorization with Manager/Director approval roles. Agent-selected migration: demo.approver becomes Manager; add two Director demo accounts; Admin retains administration only, with no approval bypass. This migration is proposed design, not participant approval.
- **BREAKING:** creation and Domain approval reject non-positive or fractional-cent USD values instead of accepting all positive decimals. Numeric trailing zeros are accepted; no rounding occurs.
- Deliver single-approver limits and two-person transitions in separate delegated tasks and commits, with mapped unit/API/concurrency/UI tests and comparable mutation evidence.

## Capabilities

### New Capabilities

- `invoice-approval`: Monetary validity, role limits, authenticated distinct-person decisions, pending progress, atomic transitions and immutable failed attempts.

### Modified Capabilities

None. The CLI capability inventory reports no existing specs.

## Impact

Domain ApprovalService, Invoice and IInvoiceRepository; the singleton in-memory repository and test fake; approval and creation controllers, role filters, demo accounts, static UI, unit/API/repository tests and Playwright page objects. Existing API routes remain; invoice JSON gains nullable first-approval fields and errors become explicit. No persistence replacement or new infrastructure is planned. Participant selected first-role eligibility and USD cents; remaining design choices require artifact review. Tests, mutation runs and participant reviews are planned, not executed.
