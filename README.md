# Invoice Approval Demo App

A small invoice approval app used for live AI-in-SDLC demos. It is deliberately simple: a .NET 8 API, a plain web UI, and an in-memory store, so it runs with one command and no database.

## Run it

```sh
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet run --project backend/src/InvoiceApp.Api
```

Open http://localhost:5000 and sign in:

| User | Password | Role |
| --- | --- | --- |
| `demo.approver` | `ChangeMe123!` | Manager |
| `demo.director1` | `ChangeMe123!` | Director |
| `demo.director2` | `ChangeMe123!` | Director |
| `demo.admin` | `ChangeMe123!` | Admin |

These are demo accounts for a local, in-memory app. They are not real credentials.

## What it does

1. Create an invoice (it starts as a draft).
2. Submit it for approval.
3. A Manager approves through USD 50,000 inclusive; a Director approves through USD 500,000 inclusive. Either may reject with a reason. Admin can delete but cannot approve or reject.

Stage one is implemented: amounts above USD 500,000 return a two-person-workflow-unavailable conflict without changes. Two-person approval is a later stage. Positive USD amounts must be numeric whole cents (trailing zeros accepted); fractional cents are rejected without rounding. All lifecycle changes are atomic and repository results are detached snapshots. Restart and sign in again after migrating accounts, since existing sessions retain their roles. The creation form preserves the entered numeric value exactly in JSON, including trailing zeros and fractional cents; the server validates it without rounding. Displayed amounts still use JavaScript numbers and can be approximate for very large values.

Invoices move through Draft, Pending Approval, then Approved or Rejected.

## Tests

```
dotnet test backend/InvoiceApp.sln
node --test backend/tests/ui/*.test.cjs
```

The dependency-free Node tests exercise the creation form submit handler and exact JSON amount transport. They are not browser end-to-end tests.

## Project layout

```
backend/
  src/InvoiceApp.Domain   business rules
  src/InvoiceApp.Api      controllers, sign-in, web UI (wwwroot)
  tests/InvoiceApp.Tests  xUnit tests
docs/use-cases            short feature briefs
AGENTS.md                 conventions for AI coding agents
```

## Use cases

- [UC-1 Bulk invoice approval](docs/use-cases/UC-1-bulk-approval.md)
- [UC-2 Partial approval](docs/use-cases/UC-2-partial-approval.md)
