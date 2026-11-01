# Assignment agent responsibilities

The participant requested Phase 2 of ACTION_PLAN.md, which calls for multiple agents and sub-agents. These task names are runtime delegations, not installed persistent Codex profiles.

| Role | Responsibility | Actual status |
| --- | --- | --- |
| Coordinator `/root` | Integrate work, preserve evidence, maintain plan and staged changes | Active through stage-two implementation |
| Fresh agent `/root/context_check` | Explain repository context with no inherited conversation | Completed read-only; exact prompt and response saved here |
| Test sub-agent `/root/context_check/test_context` | Inspect test gaps, especially distinct identity and unchanged stored state | Completed read-only, delegated by context agent; exact prompts and response saved with context evidence |
| CLI logging-check agent | Read AGENTS.md and logger; report test command and logging limitation | Real Phase 2 verification runs; session IDs/outcomes in phase evidence |
| Specification agent `/root/specification` | Inspect application, then generate specification/tasks through selected tool | Completed discovery and CLI-guided generation of all four OpenSpec artifacts; strict validation passed |
| Implementation agent `/root/stage_one` | Stage-one limits, trusted API roles, exact cents and atomic repository with tests | Implemented in 5cf4d61; participant review complete |
| Implementation agent `/root/stage_two` | Separate two-person extension with preserved regressions and pending UI | Implemented in df5fa14; participant explicitly reviewed and is satisfied with the tests |
| Test-review child `/root/stage_two/test_review` | Independent FR-04/FR-05, metadata, concurrency and UI test review | ST2-T01 coverage gap addressed and independently rechecked; report preserved |
| Test-review child `/root/stage_one/test_review` | Review real code and generated tests, reproduce issues, recheck fixes | Stage-one independent review; source/API findings and fixes recorded in stage-one-test-review.md |
| Test agent | Review feature tests and mutation survivors | Stage-one generated-test review performed by child; participant confirmed completion with `done` for commit 5cf4d61 |
| Specification reviewer `/root/spec_review` | Independently review Phase 3 artifacts, then recheck the focused correction | Completed; SPEC-01 addressed; distinct from workshop review |
| PR review agent | Use exact workshop prompt and number actionable findings | Planned for Phase 7; workshop prompt needed |
| Participant | Decide specification questions, review tests, judge review findings, record demo | Actual input: exclude images; Manager or Director first; positive USD cents. Artifact/test/finding judgments are not inferred |

Before delegation, preserve the exact prompt. Record task IDs, parent task, session references when available, tools/skills actually used, result and limitations. Keep overlapping edits sequential or isolated. Context checks do not authorize production implementation.

Artifacts: `context-check-prompt.txt`, `context-check-response.md` (including verbatim child response), `test-context-prompts.txt`, `logging-check-prompt.txt`, and `logging-check-response.md`.

## Selected skills

| Skill | Scope | Actual invocation |
| --- | --- | --- |
| OpenAI Docs | Verify supported Codex lifecycle logging | Coordinator read installed skill and official Hooks page in Phase 2 |
| Skill Creator | Build and validate narrow assignment skill | Coordinator read installed skill and ran validator in Phase 2 |
| invoice-assignment | Specification, implementation, tests, review and context-check modes | Repository `.agents/skills/invoice-assignment/SKILL.md`; actually read by context agent and test child |
| Playwright | UI rejection test with page objects and role locators | Selected for Phase 6; not yet invoked |
| OpenSpec proposal workflow | CLI-backed specification, design and task artifacts | OpenSpec 1.16.0 proposal used in Phase 3; apply-change used by coordinator/implementation agent in stage one |

AGENTS.md is context, not itself a skill. Role definitions do not prove an agent ran. This table does not replace the exact workshop review prompt. Actual relationships and human input are recorded in the interaction log.

Phase 3 evidence: `specification-discovery-prompt.txt`, `specification-discovery.md`, `specification-generation-prompt.txt`, `specification-review-prompt.txt`, `specification-review.md`, `specification-review-dispositions.md`, `specification-review-recheck-prompt.txt`, and `specification-review-recheck.md`. Actual session/parent references are preserved in task metadata and transcript exports.

Stage-one prompts/results: `stage-one-implementation-prompt.txt`, `stage-one-transport-followup-prompt.txt`, `stage-one-test-review-prompt.txt`, `stage-one-test-review.md`; coordinator evidence is in `docs/evidence/PHASE_4.md`. Participant test review is requested through `docs/evidence/phase-4/TEST_REVIEW.md`; participant later replied `done`; completion is recorded without inferring individual finding verdicts.

Stage-two prompts/results: `stage-two-implementation-prompt.txt`, `stage-two-test-review-prompt.txt`, `stage-two-test-review.md` and `stage-two-coordinator-review.md`. Phase 5 evidence and participant review guide are in `docs/evidence/PHASE_5.md` and `docs/evidence/phase-5/TEST_REVIEW.md`. Stage-one confirmation is not reused as stage-two test review or individual finding verdicts.


## Phase 6 actual work

`/root/phase_six_browser` implemented and ran the genuine Chromium Manager rejection test with accessible page objects, all-field reload equality and an owned server. `/root/phase_six_mutation` ran the pinned same-scope before/after experiment in an isolated worktree and delegated read-only `/root/phase_six_mutation/mutation_test_review` to assess actual reports and explanation assertions. Exact prompts and results are under `phase-six-*`, `playwright-implementation.md` and `mutation-investigation-*`; coordinator verification is in `phase-six-coordinator-review.md`. Parent/session references are in structured logs/transcripts.

Actual skills: Playwright and invoice-assignment testing guidance for browser work; invoice-assignment testing/review guidance for mutation parent/child; OpenSpec apply for coordinator tracking. No workshop review is implied. Participant separately answered `Reviewed; satisfied with both additions` for test input commit 437dd89; individual finding verdicts remain unsupplied.
