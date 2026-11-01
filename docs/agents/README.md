# Assignment agent responsibilities

The participant requested Phase 2 of ACTION_PLAN.md, which calls for multiple agents and sub-agents. These task names are runtime delegations, not installed persistent Codex profiles.

| Role | Responsibility | Actual status |
| --- | --- | --- |
| Coordinator `/root` | Integrate work, preserve evidence, maintain plan and staged changes | Active through Phase 3 |
| Fresh agent `/root/context_check` | Explain repository context with no inherited conversation | Completed read-only; exact prompt and response saved here |
| Test sub-agent `/root/context_check/test_context` | Inspect test gaps, especially distinct identity and unchanged stored state | Completed read-only, delegated by context agent; exact prompts and response saved with context evidence |
| CLI logging-check agent | Read AGENTS.md and logger; report test command and logging limitation | Real Phase 2 verification runs; session IDs/outcomes in phase evidence |
| Specification agent `/root/specification` | Inspect application, then generate specification/tasks through selected tool | Completed discovery and CLI-guided generation of all four OpenSpec artifacts; strict validation passed |
| Implementation agent | Separate single-approver and two-approver tasks, each with tests and commit | Planned for Phases 4–5; not yet run |
| Test agent | Review feature tests and mutation survivors | Only test-context inspection has occurred; feature review pending |
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
| OpenSpec proposal workflow | CLI-backed specification, design and task artifacts | OpenSpec 1.16.0 proposal skill invoked by coordinator/specification agent; four artifacts generated and reviewed |

AGENTS.md is context, not itself a skill. Role definitions do not prove an agent ran. This table does not replace the exact workshop review prompt. Actual relationships and human input are recorded in the interaction log.

Phase 3 evidence: `specification-discovery-prompt.txt`, `specification-discovery.md`, `specification-generation-prompt.txt`, `specification-review-prompt.txt`, `specification-review.md`, `specification-review-dispositions.md`, `specification-review-recheck-prompt.txt`, and `specification-review-recheck.md`. Actual session/parent references are preserved in task metadata and transcript exports.
