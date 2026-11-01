# Assignment agent responsibilities

The participant requested Phase 2 of ACTION_PLAN.md, which calls for multiple agents and sub-agents. These task names are runtime delegations, not installed persistent Codex profiles.

| Role | Responsibility | Actual status |
| --- | --- | --- |
| Coordinator `/root` | Integrate work, preserve evidence, maintain plan and staged changes | Active in Phases 1–2 |
| Fresh agent `/root/context_check` | Explain repository context with no inherited conversation | Completed read-only; exact prompt and response saved here |
| Test sub-agent `/root/context_check/test_context` | Inspect test gaps, especially distinct identity and unchanged stored state | Completed read-only, delegated by context agent; exact prompts and response saved with context evidence |
| CLI logging-check agent | Read AGENTS.md and logger; report test command and logging limitation | Real Phase 2 verification runs; session IDs/outcomes in phase evidence |
| Specification agent | Invoke selected spec tool; produce specification/tasks and unresolved decisions | Planned for Phase 3; not yet run |
| Implementation agent | Separate single-approver and two-approver tasks, each with tests and commit | Planned for Phases 4–5; not yet run |
| Test agent | Review feature tests and mutation survivors | Only test-context inspection has occurred; feature review pending |
| Review agent | Use exact workshop prompt and number actionable findings | Planned for Phase 7; workshop prompt needed |
| Participant | Decide specification questions, review tests, judge review findings, record demo | Actual Phase 2 input: do not commit images; no other human judgments inferred |

Before delegation, preserve the exact prompt. Record task IDs, parent task, session references when available, tools/skills actually used, result and limitations. Keep overlapping edits sequential or isolated. Context checks do not authorize production implementation.

Artifacts: `context-check-prompt.txt`, `context-check-response.md` (including verbatim child response), `test-context-prompts.txt`, `logging-check-prompt.txt`, and `logging-check-response.md`.

## Selected skills

| Skill | Scope | Actual invocation |
| --- | --- | --- |
| OpenAI Docs | Verify supported Codex lifecycle logging | Coordinator read installed skill and official Hooks page in Phase 2 |
| Skill Creator | Build and validate narrow assignment skill | Coordinator read installed skill and ran validator in Phase 2 |
| invoice-assignment | Specification, implementation, tests, review and context-check modes | Repository `.agents/skills/invoice-assignment/SKILL.md`; actually read by context agent and test child |
| Playwright | UI rejection test with page objects and role locators | Selected for Phase 6; not yet invoked |
| Spec tool workflow | Actual generated specification and task artifacts | Select/verify in Phase 3; not yet invoked |

AGENTS.md is context, not itself a skill. Role definitions do not prove an agent ran. This table does not replace the exact workshop review prompt. Actual relationships and human input are recorded in the interaction log.
