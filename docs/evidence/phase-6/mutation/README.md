# Comparable mutation experiment

Measured on detached production/test HEAD `54138b503167d1e0962f7522845856a62256d3fb`, retained at ignored `.tools/mutation-worktree`. SDK/runtime/tool records, exact commands, before/after source hashes, complete JSON and console reports are adjacent. Local tool manifest pins Stryker.NET 4.13.0; the original repository-local .NET 8 SDK is used from its absolute path. Nothing installs a global SDK/tool or builds mutation binaries in the original checkout.

## Fixed scope and settings

`stryker-config.json` at repository root selects Domain `InvoiceApp.Domain.csproj` and the complete `backend/tests/InvoiceApp.Tests/InvoiceApp.Tests.csproj`; explicit source includes are `**/ApprovalService.cs` and `**/UsdAmount.cs`. Both measured runs use Standard mutation level, Debug, vstest, concurrency 2, Json/ClearText/Html reporters, coverage analysis off, initial failure break, unchanged thresholds (display defaults 80/60; break 0). No arbitrary ignored mutation types/methods, selective tests, since/baseline shortcuts, or survivor exclusions were configured. The same exact command/configuration is used twice. HTML/temp/binaries remain ignored under `.tools`; raw JSON/text are retained here.

Coverage off was chosen before measuring to avoid matching dynamically timestamped xUnit MemberData identities. Stryker still warns about discovery names in initial runs, but all cases are executed and coverage matching/filtering is disabled. [Official configuration documentation](https://stryker-mutator.io/docs/stryker-net/configuration/#coverage-analysis-string) describes off as running all unit tests against every mutant. The [project/include documentation](https://stryker-mutator.io/docs/stryker-net/configuration/) and pinned CLI help informed the config. Setup failures/retries are preserved separately in `setup-notes.txt`, `setup-invalid-config.txt`, and `setup-coverage-warning.txt`; none is a baseline score.

## Tests and investigation

Untouched initial suite: 171 passed, zero failures/skips. After preservation of BEFORE, independent read-only child `/root/phase_six_mutation/mutation_test_review` inspected actual reports and contracts; parent `/root/phase_six_mutation`, coordinator `/root`. Exact prompt/result and handling live under `docs/agents/mutation-investigation-*`. Both used repository `invoice-assignment` testing/review guidance. No survivor/no-coverage cases were present. Child identified three meaningful explanation assertions associated with timeout mutants; these are not fabricated survivors or production bugs.

Test-only strengthening requires useful non-draft Submit reasons and complete fresh state across three statuses, a useful blank-rejection explanation with full state preservation, and an authenticated-identity explanation that cannot pass merely through ArgumentException parameter-name suffixes. Exact original content is recoverable from the clean revision; `test-strengthening.diff` preserves each change. Focused command passed 73 cases; full unit/API/repository command passed 173 cases (two additional non-draft Submit cases). No production bytes changed. Both mutation runs select the same full test project, with the intended stronger tests explaining the count increase.

## Review and limits

Agent review supports these test changes. Participant generated-test review is pending; no human judgment is inferred. The browser regression and its evidence are owned by the coordinator/browser agent. This experiment makes no claims about browser execution, publication, remote CI, workshop review, or commits.

A 100% score does not prove every possible fault absent: tool mutation level/scope, automatically ignored block mutants, timeout classification and ungenerated faults limit the result. Threshold/duplicate-Director/full-state contract tests already existed; no invented score target or deficiency was used.


## Actual comparison

| Status | BEFORE | AFTER |
| --- | ---: | ---: |
| Console score | 100.00% | 100.00% |
| Killed | 97 | 100 |
| Timeout | 3 | 0 |
| Survived | 0 | 0 |
| NoCoverage | 0 | 0 |
| CompileError / RuntimeError | 0 / 0 | 0 / 0 |
| Ignored | 35 | 35 |
| Generated | 135 | 135 |
| Tested / score denominator | 100 | 100 |

Stryker counts Killed and Timeout as detected, so BEFORE was `(97 + 3) / 100 = 100.00%`; AFTER is `100 / 100 = 100.00%`. **There is no numerical score improvement.** Meaningful improvement is that three explanatory-string mutants formerly classified Timeout are now actually Killed. No remaining surviving/no-coverage/timeout mutant needs an equivalence excuse. Mutant ids/locations/replacements and included sources match across both reports.

| Mutant id (ApprovalService.cs) | Fault | BEFORE | AFTER |
| --- | --- | --- | --- |
| 3, line 9 | Empty explanation for non-draft submit | Timeout | Killed |
| 42, line 47 | Empty explanation when rejection reason is blank | Timeout | Killed |
| 91, line 75 | Empty explanation when actor identity is blank | Timeout | Killed |

AFTER mutation execution provides direct evidence of detection of those faulty behaviors without manually editing production. `strengthened-mutant-detection.json` maps each `killedBy` id from the raw AFTER report directly to the strengthened Submit, blank-reason and invalid-identity tests. Timeout was not characterized as assertion detection or a baseline survivor. The changed cases make the useful-message contract explicit; runtime variation could also affect timeout classification, so no claim is made that test changes alone mathematically account for all timing behavior.

Both reports retain 35 ignored mutants: 24 outside the explicit two-file scope (22 DiscountCalculator, 2 Invoice), plus Stryker's 11 automatic block-already-covered filters (10 ApprovalService, 1 UsdAmount). These were not manually ignored to increase the score. Included tested counts remain ApprovalService 92 and UsdAmount 8. All 100 included active mutants are killed AFTER. No surviving behavior was hidden or removed from scope.

Validated source hashes show only `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs` differs before/after; all production source, local tool manifest and final config hashes match. Test changes were copied to the original checkout after review and both measured runs. The detached worktree is retained for reproducibility; raw HTML remains local and ignored. Coordinator owns integration/commit and browser verification.
