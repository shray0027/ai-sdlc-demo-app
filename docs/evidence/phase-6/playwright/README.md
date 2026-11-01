# Phase 6 browser execution evidence

On 2026-11-01, Chromium passed the single Manager-over-limit test against production revision `54138b503167d1e0962f7522845856a62256d3fb`: **1 passed (3.8s)**. Production files had no diff against that revision. The run used Node 24.18.0, npm 11.16.0, repository-local .NET SDK 8.0.427, Playwright 1.63.0 from the unchanged lockfile, and Chromium 153.0.8010.12 (build v1243).

Actual setup/check commands, from the repository root unless `cd` is shown:

```sh
command -v npx
export PATH="/Users/shrayanand/.nvm/versions/node/v24.18.0/bin:$PATH"
export DOTNET_ROOT="$PWD/.tools/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
node --version
npm --version
dotnet --version
npm ci --prefix e2e > docs/evidence/phase-6/playwright/npm-ci.txt 2>&1
cd e2e
npx playwright install chromium > ../docs/evidence/phase-6/playwright/chromium-install.txt 2>&1
npm test > ../docs/evidence/phase-6/playwright/test-output.txt 2>&1
```

`npm test` was executed twice. The first attempt failed because the test read session storage before asynchronous login completed: the UI's existing CSS makes the Sign out button visible early, so button visibility alone was an insufficient wait. `initial-attempt.txt` and `initial-results.json` preserve that actual failure. The page object now polls only the browser session's username and role until the real login completes. The second invocation passed. No production change was made for this test synchronization issue.

The config asks the OS for a free `127.0.0.1` port and propagates it to Playwright workers. Playwright starts its own server with this command (the successful run selected port 51141):

```sh
dotnet run --no-launch-profile --project backend/src/InvoiceApp.Api -- --urls http://127.0.0.1:51141
```

`reuseExistingServer: false` prevents use of a pre-existing app. Playwright tears down its server. A Python socket `connect_ex` check after test completion verified that the selected endpoint no longer had a listener; `environment.json` records the result, revision, tracked production-file SHA-256 values and dependency-lock hash. No command targeted or stopped a server on localhost:5000. There is a short allocation-to-bind race, as with dynamic port selection generally; a competing listener causes failure instead of reuse.

Evidence artifacts:

- `test-output.txt`: final successful console output.
- `results.json`: Playwright's final JSON reporter output.
- `invoice-state.json`: synthetic invoice from fresh authenticated API reads before the UI approval attempt and after reload; all 11 fields match, including null first/final approval metadata. It also records HTTP 409 and the asserted visible reason.
- `environment.json`: versions, production revision/hashes and server teardown verification.
- `npm-ci.txt`, `chromium-install.txt`: actual dependency/browser setup output.
- `initial-attempt.txt`, `initial-results.json`: initial test synchronization failure, retained for transparency.

Runtime HTML reports are ignored under `e2e/playwright-report/`; screenshots and error contexts belong under ignored `e2e/test-results/`. The failed run initially produced a screenshot there; the successful rerun replaces runtime results. No images are submitted. Video and traces are disabled; network traces could contain session bearer headers. The API-read helper keeps the token inside the browser and returns only the invoice. Submitted evidence contains synthetic invoice data and no bearer token.

The implementation agent inspected the assertions and evidence. This verifies one actual Manager rejection through the browser; it does not claim browser coverage for other roles, thresholds or dual-approval flows, nor participant review. Persisted state here is the application's in-memory repository, freshly read over authenticated HTTP; durability across server restarts is outside this application/test.


## Final coordinator integration run

After the implementation run above, the coordinator validated the configurable port before shell interpolation and reran the browser test with the combined strengthened test inputs committed as `437dd899f782a937da38cb58bb517cd40207aeb1`. Production bytes remained identical to the original base. The final browser result is **1 passed (2.0s)**; the complete integration run also passed **173 Release .NET** and **46 Node** tests, with a zero-warning Release build.

`agent-results.json` and `agent-invoice-state.json` preserve the initial successful agent run described above. `results.json` and `invoice-state.json` now describe the final coordinator run. `coordinator-run.json` records its dedicated endpoint and confirms teardown. Full output is `../final-checks.txt`. Original setup/failure records and `environment.json` remain intact; no token or image was added to committed evidence.
