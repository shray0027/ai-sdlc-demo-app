# AI in SDLC assignment — invoice approval limits

Status: both implementation stages and Phase 6 technical verification are complete. Participant reviews of stage-one/stage-two tests and Phase 6 browser/strengthened tests are recorded. PR workshop review and the demo remain unfinished.

- Fork: https://github.com/shray0027/ai-sdlc-demo-app
- Branch: `assignment/invoice-approval-limits`
- PR URL: not created yet (Phase 7).
- Demo video: not recorded/uploaded yet; an accessible 8–10-minute MP4 link is still required.

## Feature and decisions

Managers finally approve through **USD 50,000 inclusive**; Directors through **USD 500,000 inclusive**. Above that, a Manager or Director records the first approval while the invoice remains PendingApproval. A different canonical authenticated user with Director role must approve second. A Director cannot approve twice, even after another login/token or a role change. Failed approvals return clear reasons and commit no invoice/history changes.

The participant explicitly chose Manager-or-Director first eligibility and positive USD amounts with at most two decimal places. Numeric trailing zeros are accepted; fractions are rejected without rounding. Agent-selected design uses canonical server-session username identity, nullable first approval identity/role/UTC time, Manager/Director approval authority without an Admin bypass, and copy-on-write repository transitions. These design choices are distinguished from participant decisions in [the decision record](docs/decisions/approval-policy.md).

The .NET 8 Domain retains policy, the authenticated API supplies authority, and the singleton in-memory repository serializes mutations under one lock. Detached copies and rollback on exceptions prevent leaked/partial state. Exact JSON parsing and exact numeric form transport prevent rounding before validation. First history survives final approval and intentional rejection. UI progress uses server metadata. Persistence survives requests/reloads, not application restarts; very large returned values can be displayed approximately by JavaScript formatting.

## Specification and incremental delivery

OpenSpec **1.16.0** scaffolded the change and supplied artifact-specific instructions/templates; the delegated specification agent authored the proposal, delta, design and tasks through that workflow. [Specification artifacts](openspec/changes/invoice-approval-limits/) and [CLI evidence](docs/evidence/openspec/README.md) are preserved. Independent review clarified temporary stage-one high-value denial tests versus their final two-person replacements; strict validation passed.

| Revision | Purpose |
| --- | --- |
| `ee55d6c` | Repository/baseline evidence, transcribed requirements, images excluded |
| `1475b49` | Agent context, logging and responsibilities |
| `63c2197` | Generated/reviewed specification artifacts |
| `5cf4d61` | Single-approver limits, exact cents and atomic immutable failures |
| `df5fa14` | Separate two-distinct-approver extension and pending UI |
| `437dd899f782a937da38cb58bb517cd40207aeb1` | Browser rejection regression, pinned mutation tooling and strengthened failure assertions |

Human test-review acknowledgments are separately recorded in `e869ce4` and `54138b5`. The two implementation prompts/commits remain separate. Requirements/source screenshots stay outside the submitted history; transcribed requirements are included.

## AI context, skills, hooks and delegation

[AGENTS.md](AGENTS.md) records all five rules, layering, commands, immutable-state expectations and review requirements. A fresh agent with no inherited conversation explained the rules/layers/commands/change locations and delegated a child test-gap check; [context evidence](docs/agents/context-check-response.md) shows the guidance mattered.

Actually used skills: invoice-assignment; generated OpenSpec propose/apply; Playwright for the explicitly requested browser test; Skill Creator for the assignment skill; OpenAI Docs during hook setup. The [agent record](docs/agents/README.md) distinguishes actual invocations from planned ones.

Coordinator `/root` integrated changes/evidence. Specification and independent specification-review agents produced/reviewed the plan. Separate stage-one/stage-two agents each delegated a read-only test reviewer. Phase 6 used a browser agent and a mutation agent; the latter delegated a child to inspect the preserved reports before strengthening tests. Exact prompts, follow-ups, outcomes and parent/session identifiers are preserved in `docs/agents/`, structured task metadata and available transcript exports.

`.codex/hooks.json` and `.codex/config.toml` configure local lifecycle capture. A real CLI task verified SessionStart, UserPromptSubmit, PostToolUse and Stop delivery using reviewed invocation-only inline settings. Automatic hook activation in the existing IDE session was not verified; explicit export of Codex's saved transcripts is the working capture path there. [Logging documentation](logs/README.md), [structured task records](logs/interactions.jsonl), [readable summary](logs/SUMMARY.md), and transcript exports preserve provenance. Initial reconstructed setup notes remain labeled; exported text/tool events are not presented as private/system content or a guarantee of future capture.

The log evidence covers the nine suggested categories: tool/model; agent/type/mode; prompts/follow-ups; skills/instructions/sub-agents; tools/integrations; actions/outputs; actual human intervention; timestamps/files/results/session/revision references; and outcome. Model identifiers are observed session metadata, not invented aliases. No external logging/dashboard upload is claimed.

## Human intervention and review evidence

- The participant requested exclusion of images; the unpushed baseline was amended and ignore rules added.
- The participant resolved the two material policy questions rather than the agent silently selecting them.
- Stage-one test review was confirmed with exact reply `done` for `5cf4d61`.
- Stage-two review was explicitly confirmed: `Reviewed; satisfied with the tests` for `df5fa14`.
- Phase 6 review for `437dd89` was explicitly confirmed: `Reviewed; satisfied with both additions`. This is its own review, not reused from earlier confirmations.

Agent findings led to concrete fixes: exact JSON and browser-input rounding protection; independent expected-state oracles; complete race response assertions; explicit transition-test replacement; and populated-first-history denial coverage. Phase 6 review identified meaningful error-explanation assertions even though no mutant survived. Agent handling/rechecks are preserved; individual participant finding verdicts and the exact workshop PR review remain pending. No human judgment is fabricated from an agent conclusion.

## Verification

| Stage | Actual result |
| --- | --- |
| Original baseline | 7 .NET tests passed after local .NET 8 setup |
| Single-approver stage | 98 .NET and 42 Node cases passed |
| Two-person stage | 171 .NET and 46 Node cases passed |
| Phase 6 final | **173 Release .NET**, **46 Node**, **1 Chromium Playwright test** passed; Release build zero warnings/errors |

[Final commands/output](docs/evidence/phase-6/final-checks.txt) and [Phase 6 evidence](docs/evidence/PHASE_6.md) identify exact inputs. Local checks include the commands wired into CI; remote CI execution is not claimed.

The [browser test](e2e/tests/manager-limit.spec.ts) signs in as Manager, creates/submits a unique USD 50,000.01 invoice through UI, observes the exact limit reason/409, reloads and compares all 11 persisted fields via fresh authenticated HTTP. It uses role/label page objects and an owned loopback server. [Browser evidence](docs/evidence/phase-6/playwright/README.md) preserves initial synchronization failure, successful runs and server teardown. No image, video, bearer token or network trace is submitted.

## Mutation scores — before and after

**Before test strengthening: 100.00%.** 97 Killed, 3 Timeout, 0 Survived, 0 NoCoverage, 35 Ignored; 135 generated, 100 active/tested.

**After test strengthening: 100.00%.** 100 Killed, 0 Timeout, 0 Survived, 0 NoCoverage, 35 Ignored; 135 generated, 100 active/tested.

There is **no numerical score improvement**. Stryker counted the three before timeouts as detected. After strengthening, those three explanation mutants were actually killed; [killedBy mappings](docs/evidence/phase-6/mutation/strengthened-mutant-detection.json) tie them to the new assertions. Timeout is not described as a baseline survivor or proven assertion detection, and runtime variation is not excluded as a timing factor.

| Mutant | Strengthened contract |
| --- | --- |
| ApprovalService id 3, line 9 | Useful non-draft submit reason and complete fresh state across three statuses |
| id 42, line 47 | Useful blank rejection-reason explanation and unchanged state |
| id 91, line 75 | Useful authenticated-identity explanation rather than a matching parameter suffix |

The actual experiment ran on detached base **54138b503167d1e0962f7522845856a62256d3fb**, first with original 171 tests, then with only the [preserved test diff](docs/evidence/phase-6/mutation/test-strengthening.diff) applied (173 tests). Worktree HEAD stayed at that base; the matching AFTER test/tooling inputs are committed in **437dd899f782a937da38cb58bb517cd40207aeb1**. No claim is made that the experiment's detached HEAD changed to the later integration commit. Source hashes prove the committed tests match the measured AFTER and all production bytes remain unchanged.

Fixed settings: Stryker.NET **4.13.0**, .NET 8 SDK 8.0.427, Domain `ApprovalService.cs` and `UsdAmount.cs`, Standard, Debug, vstest, concurrency 2, complete test project, coverage analysis off, same reporters/config/tool bytes. Coverage off avoids mismatching dynamically timestamped test-data IDs and runs the whole suite per mutant; see [Stryker configuration documentation](https://stryker-mutator.io/docs/stryker-net/configuration/). No target score was specified; 80/60 are tool display defaults and break is 0, not an assignment gate.

Same 135 mutant fingerprints/report source text were independently compared. Ignored counts are unchanged: 24 outside the explicit two-file scope and 11 automatic block-covered filters; no survivor exclusions were added. There are no remaining AFTER survivors/no-coverage/timeouts/compile/runtime errors. A focused score does not cover ungenerated faults or every API/storage/UI line.

[Complete mutation evidence and exact commands](docs/evidence/phase-6/mutation/README.md), [raw BEFORE JSON](docs/evidence/phase-6/mutation/before-report.json), [raw AFTER JSON](docs/evidence/phase-6/mutation/after-report.json), hashes and setup retries are preserved. The detached worktree stays under ignored `.tools/mutation-worktree`; HTML/temp/binaries stay local. Local tool manifest pins the version; `dotnet tool restore` restores it. The reproducible mutation command from the documented isolated worktree is:

```sh
dotnet stryker --config-file stryker-config.json --output .tools/stryker-output --skip-version-check
```

## Remaining submission work

The exact workshop review prompt, PR creation/review, every finding's participant verdict, final write-up/links and an uploaded accessible 8–10-minute MP4 remain pending. The video must show live execution; reading these files alone is insufficient. [ACTION_PLAN.md](ACTION_PLAN.md) tracks remaining work. This write-up is a current evidence record, not a claim that the assignment is already submitted or complete.
