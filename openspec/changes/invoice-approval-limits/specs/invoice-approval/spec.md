# Spec Delta

## Purpose

Control invoice approval by monetary authority and distinct authenticated people while preserving complete state on failed decisions, including concurrent attempts.

## ADDED Requirements

### Requirement: FR-01 Manager final approval limit
The system SHALL allow a Manager to finally approve a pending valid USD invoice up to and including 50,000. A Manager SHALL NOT finally approve any amount above that limit alone.

#### Scenario: Manager below and at inclusive limit
- **GIVEN** separate pending invoices of USD 49,999.99 and 50,000.00
- **WHEN** an authenticated Manager approves each
- **THEN** each becomes Approved with that Manager and a UTC final decision time

#### Scenario: Manager above limit in single-approval range
- **GIVEN** a pending invoice of USD 50,000.01 or 500,000.00
- **WHEN** a Manager attempts approval
- **THEN** the attempt fails with a clear Manager-limit reason and complete unchanged state

### Requirement: FR-02 Director final approval limit
The system SHALL allow a Director to finally approve a pending valid USD invoice up to and including 500,000. Above that limit a Director SHALL NOT finally approve alone.

#### Scenario: Director below and at inclusive limit
- **GIVEN** separate pending invoices of USD 50,000.01, 499,999.99 and 500,000.00
- **WHEN** an authenticated Director approves each
- **THEN** each becomes Approved with that Director and a UTC final decision time

#### Scenario: Director above inclusive limit
- **GIVEN** a pending invoice of USD 500,000.01 with no first approval
- **WHEN** a Director approves
- **THEN** only the first approval is recorded and the invoice remains PendingApproval

### Requirement: FR-03 High-value first approval exception and Director completion
For valid USD amounts above 500,000 the system SHALL accept a Manager or Director as first approver, as an exception to final-approval limits. It SHALL require a distinct Director as second approver before final approval.

#### Scenario: Manager first and distinct Director second
- **GIVEN** a pending USD 500,000.01 invoice with no approvals
- **WHEN** Manager A approves and then Director B approves
- **THEN** A's first approval leaves it pending and B's second approval makes it Approved with B as final decision maker

#### Scenario: Director first and distinct Director second
- **GIVEN** a pending USD 500,000.01 invoice with no approvals
- **WHEN** Director A approves and then Director B approves
- **THEN** it becomes Approved only after B's approval and retains A's first approval metadata

#### Scenario: Distinct Manager cannot complete
- **GIVEN** a high-value invoice with a valid first approval by another user
- **WHEN** a distinct Manager attempts the second approval
- **THEN** the attempt fails with a clear Director-required reason and complete unchanged state

### Requirement: FR-04 Duplicate identities never count twice
The system SHALL compare canonical authenticated usernames using the existing exact username semantics. One username SHALL NOT count as both approvers, regardless of role, session or token.

#### Scenario: Same Director tries twice
- **GIVEN** Director A has recorded the first high-value approval
- **WHEN** Director A attempts a second approval
- **THEN** the attempt fails with a clear distinct-approver reason and complete unchanged state

#### Scenario: Same Director obtains another session
- **GIVEN** Director A has recorded the first high-value approval
- **WHEN** A logs in again, obtains another token and attempts the second approval
- **THEN** the same canonical username is recognized and the attempt fails without changing state

#### Scenario: Same identity with different role
- **GIVEN** a Manager has recorded a first approval
- **WHEN** an authenticated Director session with the same canonical username attempts completion
- **THEN** the attempt fails because the identities are equal, regardless of role change

### Requirement: FR-05 Failed approvals preserve complete state
Every failed approval SHALL return a clear reason and leave all invoice fields and approval records unchanged. This includes identity, customer, amount, status, all creation/decision/approval metadata, notes and any version fields. Missing invoices SHALL NOT be created. No failed attempt SHALL append records, clear metadata or change timestamps.

#### Scenario: All failed approval branches preserve persisted state
- **GIVEN** an independently captured complete invoice snapshot
- **WHEN** approval fails for an over-limit role, unsupported role, duplicate identity, invalid second role, invalid amount, blank identity, or Draft/Approved/Rejected state
- **THEN** a fresh stored read equals the snapshot in every field and record, and the response identifies the failure reason

#### Scenario: Missing invoice remains absent
- **GIVEN** an invoice identifier does not exist
- **WHEN** an authenticated eligible user attempts approval
- **THEN** a clear not-found response is returned and no invoice or approval record is created

### Requirement: Numeric USD cents validation
Creation and approval SHALL accept only positive USD amounts that are numerically whole cents. Numeric trailing zeros SHALL be accepted. Unsupported fractions SHALL fail without rounding; approval SHALL also reject malformed preexisting or injected amounts.

#### Scenario: Accepted cents and trailing zeros
- **GIVEN** amounts 0.01, 50000.00 and 50000.000
- **WHEN** each is created and later presented for approval
- **THEN** amount validation accepts the exact numeric value and normal role rules determine approval

#### Scenario: Unsupported precision rejected at creation
- **GIVEN** a creation request for 50000.001 or 0.001, or a non-positive amount
- **WHEN** creation is attempted
- **THEN** a clear invalid-amount reason is returned and no invoice is added or rounded

#### Scenario: Malformed existing amount rejected in approval
- **GIVEN** a preexisting or injected pending invoice has 500000.001, zero or a negative amount
- **WHEN** a Manager or Director attempts approval
- **THEN** approval fails for invalid amount without changing any field or rounding into another approval tier

### Requirement: Authenticated authority and migrated roles
Approval identity and role SHALL come only from the authenticated server session. Only Manager and Director SHALL approve or intentionally reject invoices. Admin SHALL retain administration authority without approval authority or bypass; legacy Approver SHALL have no implicit approval authority.

#### Scenario: Client identity and role spoofing has no authority
- **GIVEN** an authenticated Manager session
- **WHEN** the client supplies another username or Director role in an approval payload or edits browser session metadata
- **THEN** the server uses only the authenticated Manager identity and role and enforces its limits

#### Scenario: Unauthenticated or unsupported role denied
- **GIVEN** no valid session, or an Admin, legacy Approver or unknown role session
- **WHEN** approval is attempted
- **THEN** it fails with a clear authentication or authorization reason and unchanged invoice state

#### Scenario: Demo role migration
- **GIVEN** the migrated demo configuration
- **WHEN** demo.approver and two distinct Director accounts sign in
- **THEN** demo.approver has Manager authority, each Director has its own canonical username, and demo.admin has only Admin authority

### Requirement: Pending progress and historical metadata
Invoice JSON responses SHALL expose nullable firstApprovedBy, firstApprovedRole and firstApprovedAt. All three SHALL be null before a first high-value approval and populated together afterward. decidedBy and decidedAt SHALL stay null until a final approval or intentional rejection. Final completion or intentional rejection SHALL preserve first-approval metadata.

#### Scenario: First approval preserves pending state
- **GIVEN** a high-value pending invoice with null first and final fields
- **WHEN** an eligible first approver succeeds
- **THEN** status remains PendingApproval, first identity/role/UTC time are set together, and decidedBy, decidedAt and decisionNote remain null

#### Scenario: Single-approval final decision
- **GIVEN** a valid invoice within the acting role's final limit
- **WHEN** approval succeeds
- **THEN** final identity/time are set while all first-approval fields remain null

#### Scenario: Intentional rejection after first approval
- **GIVEN** a high-value invoice awaiting its second approval
- **WHEN** a Manager or Director intentionally rejects it with a nonblank reason
- **THEN** status becomes Rejected with final rejecting identity/time/reason and unchanged first-approval metadata

#### Scenario: UI explains progress
- **GIVEN** a successful first high-value approval
- **WHEN** the UI refreshes or reloads
- **THEN** it displays that a second Director approval is required and identifies the recorded first approver

### Requirement: Atomic concurrent transitions
Concurrent approvals, submission, intentional rejection and deletion SHALL behave as a valid serial ordering for each invoice. Failed operations SHALL commit no partial state. Reads and returned results SHALL be stable snapshots that callers cannot mutate to alter stored invoices.

#### Scenario: Simultaneous attempts by same Director
- **GIVEN** a high-value pending invoice with no approvals
- **WHEN** two concurrent approval requests use the same Director identity
- **THEN** exactly one records a first approval, the other fails as duplicate, and final approval is absent

#### Scenario: Competing distinct first and final approvals
- **GIVEN** concurrent eligible requests for a high-value pending invoice
- **WHEN** the requests are processed
- **THEN** results match a serial order with at most one first record and one distinct Director final decision, without lost or overwritten metadata

#### Scenario: Approval competes with rejection or deletion
- **GIVEN** an approval races with an intentional rejection or Admin deletion
- **WHEN** both operations finish
- **THEN** results match a serial order with no mixed final decisions, partial approval metadata or deleted invoice resurrection

#### Scenario: Detached snapshot and exception isolation
- **GIVEN** an invoice read or mutation result returned to a caller
- **WHEN** the caller modifies it, or a transition throws after modifying its working state
- **THEN** stored state remains unchanged by those modifications and later reads return consistent snapshots

### Requirement: Clear API and UI rejection contract
Existing invoice and approval routes SHALL remain available. Failed approval responses SHALL contain JSON with a nonempty `error` string, including authentication middleware and role-filter denials: 401 for missing/invalid session, 403 for unsupported role, 404 for missing invoice, 400 for invalid amount/identity, and 409 for lifecycle, limit, duplicate or wrong-second-role conflicts. The UI SHALL display decision errors visibly.

#### Scenario: Manager rejection survives reload
- **GIVEN** a pending USD 50,000.01 invoice and a signed-in Manager
- **WHEN** the Manager uses Approve in the UI
- **THEN** a visible limit reason appears, the action has no stored effect, and a reload plus fresh read matches the complete pre-attempt snapshot
