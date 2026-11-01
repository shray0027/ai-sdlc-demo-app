# Phase 1 — repository and baseline

Recorded on 1 November 2026 (Asia/Kolkata).

- Fork: https://github.com/shray0027/ai-sdlc-demo-app
- Upstream: https://github.com/NITIN1670/ai-sdlc-demo-app
- Upstream revision: `66cb3f9005af1feafb7890f06bbc41fd652b432a`
- Assignment branch: `assignment/invoice-approval-limits`
- Checkout: `ai-sdlc-demo-app/` beneath the original planning workspace.
- Transcribed requirements are included in `requirements/`. Source images remain outside version control, as requested by the participant.

## Architecture and existing behavior

Retain ASP.NET Core 8 and the existing layering:

| Layer | Responsibility and findings |
| --- | --- |
| Domain | `ApprovalService` owns submit/approve/reject; `Invoice` stores decimal amount, lifecycle status, creation and decision metadata. No approval role or first-approval history exists. |
| API | Controllers delegate lifecycle operations to Domain. `ApprovalsController` currently allows Approver/Admin and supplies only the authenticated username. Authentication stores a canonical configured username and one role in a server-side session. |
| Persistence | Singleton `InMemoryInvoiceRepository`; records survive requests but not app restarts. It returns live invoice references, making validation before any mutation essential for FR-05. Repository calls lock independently; a two-step approval implementation needs to consider concurrent requests. |
| UI | Static HTML/CSS/JavaScript in `backend/src/InvoiceApp.Api/wwwroot`. |
| Unit tests | xUnit project with seven existing approval service tests. |
| Browser tests | Playwright config and empty page/test directories. No baseline browser tests exist. |

Existing amount validation accepts positive .NET decimals without a two-decimal restriction. Currency is unspecified by the API; inspect UI formatting during specification. API creation has no approval-role policy. Role migration, high-value first-approver eligibility, pending representation, and atomic approval transitions need specification before implementation.

## Environment and reproducible commands

The host has .NET SDKs 9.0.101 and 10.0.101, but initially had no .NET 8 runtime. The initial `dotnet test backend/InvoiceApp.sln` restored and compiled, then aborted at testhost launch. This was an environment failure, not a failing assertion.

Install .NET 8 locally without changing host SDKs (the downloaded installer and SDK are ignored by Git):

```sh
mkdir -p .tools
curl -fsSL https://dot.net/v1/dotnet-install.sh -o .tools/dotnet-install.sh
bash .tools/dotnet-install.sh --channel 8.0 --install-dir .tools/dotnet --no-path
```

This execution installed SDK **8.0.427** and ASP.NET Core/.NET runtime **8.0.33**, macOS arm64. The ASP.NET runtime was also installed explicitly before the SDK. Node is 25.5.0. No framework upgrade or application behavior change was needed.

From the application repository root:

```sh
.tools/dotnet/dotnet test backend/InvoiceApp.sln
.tools/dotnet/dotnet run --project backend/src/InvoiceApp.Api
```

For tools that invoke `dotnet` by name, including Playwright, prepend the local installation:

```sh
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
cd e2e
npm ci
npm test -- --list
```

## Actual results

- Unit suite: **7 passed, 0 failed, 0 skipped**. Raw output: [baseline-unit-tests.txt](baseline-unit-tests.txt).
- Dependency install: `npm ci` succeeded; 5 packages, 0 reported vulnerabilities; existing lockfile retained.
- Browser discovery: `npm test -- --list` returned exit 1, **No tests found**, 0 tests in 0 files. This is the upstream baseline, not successful browser validation. Browser binaries were not installed in this phase.
- Startup: app served `http://localhost:5000/` with HTTP 200.
- Authentication and invoice-list smoke check: HTTP 200, five seeded invoices. [Sanitized result](baseline-smoke.json). No invoice mutations were performed.

## Evidence and remaining phases

[Interaction record](../../logs/interactions.jsonl) reconstructs setup actions from this conversation and is explicitly labeled. It is not automatic transcript capture. Hook setup and automatic logging verification remain Phase 2 work. No sub-agent, participant review, spec generation, feature implementation, mutation run, PR review, or demo is claimed by this baseline.

The workshop review prompt is not present in the checked-out repository; obtain the exact prompt before Phase 7. Preserve participant decisions and test/review judgments as actual human input, not agent-authored claims.
