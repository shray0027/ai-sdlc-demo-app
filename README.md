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

Above USD 500,000, a Manager or Director records the first approval while the invoice remains PendingApproval. A distinct Director completes approval. Canonical usernames must differ, even across logins or role changes. First approver identity, role and UTC time remain in the invoice after completion or intentional rejection; final decision fields remain null until a final decision. Pending progress is shown from server metadata after refresh or reload. Positive USD amounts must be numeric whole cents (trailing zeros accepted); fractional cents are rejected without rounding. All lifecycle changes are atomic and repository results are detached snapshots. Restart and sign in again after migrating accounts, since existing sessions retain their roles. The creation form preserves the entered numeric value exactly in JSON, including trailing zeros and fractional cents; the server validates it without rounding. Displayed amounts still use JavaScript numbers and can be approximate for very large values.

Invoices move through Draft, Pending Approval, then Approved or Rejected.

## Tests

```
dotnet test backend/InvoiceApp.sln
node --test backend/tests/ui/*.test.cjs
```

The dependency-free Node tests exercise the creation form submit handler, exact JSON amount transport, pending approval rendering and visible decision errors. They are not browser end-to-end tests.

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


## Browser regression and mutation evidence

With Node 24 and .NET 8 selected, run the real browser rejection check:

```sh
npm ci --prefix e2e
cd e2e
npx playwright install chromium
npm test
```

The test starts and stops its own loopback server, creates a unique synthetic invoice and verifies all persisted fields after a Manager rejection and reload. It does not reuse port 5000. Optional `INVOICE_E2E_PORT` selects a dedicated free port; an existing listener causes failure rather than reuse.

Stryker.NET is pinned to 4.13.0 in the local tool manifest. `dotnet tool restore` restores it. The comparable mutation runs and exact isolated-worktree commands are in [mutation evidence](docs/evidence/phase-6/mutation/README.md). BEFORE and AFTER scores are both **100.00%**; confirmed kills increased from **97 to 100**, while timeouts decreased from **3 to 0**. No numerical score increase is claimed.

## Assignment demo preparation

See [ASSIGNMENT.md](ASSIGNMENT.md), the [nine-minute runbook](docs/DEMO_RUNBOOK.md), and [submission checklist](docs/SUBMISSION_CHECKLIST.md). Run `python3 scripts/demo_rehearsal.py --serve` for a separate owned loopback app with fresh synthetic UI invoices; Ctrl+C stops it. The participant recording/upload/share link remains pending.
