# Action Plan — AI in SDLC Assignment

Source: [requirements/REQUIREMENTS.md](requirements/REQUIREMENTS.md).

The application is checked out in `ai-sdlc-demo-app/` from the participant fork, on `assignment/invoice-approval-limits`. Phases 1–4 are complete, including participant confirmation of stage-one test review for commit `5cf4d61`. Phase 5 is complete, including explicit participant satisfaction with the stage-two tests for `df5fa14`. Phase 6 and subsequent phases remain pending. Baseline evidence: [BASELINE.md](docs/evidence/BASELINE.md). This plan covers execution through the reviewed pull request, write-up, and demo.

## 1. Establish the repository and baseline

- [x] Fork the assignment reference repository into the participant's GitHub account.
- [x] Clone the fork into a dedicated application directory and create an assignment branch. Keep the existing requirements and images available; include the requirements in the submission repository.
- [x] Inspect the application architecture, approval service, persistence, UI, tests, runtime, and existing agent instructions.
- [x] Retain the existing language and architecture unless inspection reveals a concrete reason to change them.
- [x] Install the repository's dependencies, start the application, and run its existing tests.
- [x] Record the actual setup/start/test commands and baseline results, including any pre-existing failures.

**Exit evidence:** working fork and branch, known application layers, reproducible commands, and recorded baseline results.

## 2. Set up context, logging, and agent responsibilities

Start capturing interactions as early as tooling permits. Preserve available transcripts for earlier planning/setup work, and label any reconstructed entries rather than presenting them as automatic captures.

- [x] Extend `AGENTS.md` with all five approval rules, architecture boundaries, test commands, and review expectations.
- [x] Select appropriate skills for specification, implementation, testing, and review based on the chosen environment; record which are actually invoked.
- [x] Configure supported hooks to capture interactions in a consistent format. Prefer JSONL plus a readable summary.
- [x] Verify the logging setup with one real interaction before implementation starts.
- [x] Use multiple agents and sub-agents with concrete responsibilities, as outlined below.
- [x] Give a fresh agent the repository context and ask it to explain the rules, relevant layers, test command, and planned change locations. Preserve the response as evidence that `AGENTS.md` is useful.

| Responsibility | Work and evidence |
| --- | --- |
| Coordinator agent | Maintains the plan, delegates tasks, integrates changes, and links logs to artifacts and commits. |
| Specification agent | Produces specification/tasks through the selected spec-driven tool and identifies unresolved rule details. |
| Implementation agent | Implements the single-approver and two-approver stages in separate tasks. |
| Test sub-agent | Reviews boundary/negative cases, checks generated tests, and investigates surviving mutants. |
| Review agent | Reviews the final diff using the workshop prompt and produces actionable findings. |
| Participant | Reviews generated tests, resolves specification questions, judges every review finding, and records the demo. |

These are responsibilities rather than a requirement for five simultaneously running agents. Record the actual agent/sub-agent relationships and delegated tasks; keep overlapping edits sequential or isolated.

The log should capture the nine categories from the requirements: tool/model; agent/type/mode; exact prompts and follow-ups; skills/custom instructions/sub-agents; tools/integrations; actions/outputs; human intervention; timestamps/files/test results/session and commit references; and outcomes. Use related interaction IDs and parent task references for delegation. Record only capabilities and human decisions that actually occurred.

**Exit evidence:** updated context, agent definitions or task records, functioning logging hooks, a real sample log entry, and a fresh-agent context check. Completed evidence: [PHASE_2.md](docs/evidence/PHASE_2.md). Native CLI hooks verified using inline configuration; existing IDE/project-file delivery remains unverified and uses explicit transcript export.

## 3. Generate and review the specification

- [x] Select one of the assignment's spec-driven tools: Spec Kit, OpenSpec, or BMAD. Check its supported workflow during execution. Selected OpenSpec 1.16.0; initialized Codex integration and completed CLI-guided generation after participant answers.
- [x] Generate the specification and task breakdown through that tool; preserve and commit its generated files.
- [x] Map FR-01 through FR-05 to acceptance criteria and planned tests.
- [x] Resolve the details below before implementing behavior that depends on them. Consult existing repository behavior first and record any remaining assumptions explicitly for participant review.

| Question | Decision required |
| --- | --- |
| Invoice amounts | Currency, decimal representation, and existing amount validation. |
| First approval above 500,000 | Which roles may provide it, and how this exception relates to single-approver limits. |
| Pending approval | How the first valid approval is stored and what the UI/API returns before final approval. |
| User identity | Which stable user identifier establishes that two approvers are distinct. |
| Rejected attempt | Which invoice fields and related approval records must remain unchanged. |

**Exit evidence:** reviewed specification and tasks, explicit decisions, and acceptance criteria that preserve all five rules. Completed evidence: [PHASE_3.md](docs/evidence/PHASE_3.md). OpenSpec strict validation passed, all four planning artifacts complete, and SPEC-01 clarified/rechecked. Implementation remains pending.

## 4. Implement stage one: single-approver limits

Use a dedicated implementation prompt for this stage. Do not include the complete two-approver implementation in the same request.

- [x] Implement Manager approval through 50,000 and Director approval through 500,000, inclusive.
- [x] Reject over-limit single-approver attempts with a clear reason and no invoice mutation.
- [x] Add and review tests for successful approvals, inclusive boundaries, and rejected attempts.
- [x] Ensure the rule is enforced in the approval service rather than relying solely on UI restrictions.
- [x] Run the relevant unit tests and existing regression suite.
- [x] Commit this stage with a message explaining the limits and the reason for enforcing them.

**Exit evidence:** a self-contained implementation commit with passing tests and corresponding interaction logs. Technical evidence: [PHASE_4.md](docs/evidence/PHASE_4.md). Debug/Release: 98 .NET cases pass; exact-form transport: 42 Node cases pass. OpenSpec task 2.8 is complete: participant replied `done` after the test-review request. Stage two is not implemented.

## 5. Implement stage two: two distinct approvers

Use a separate implementation prompt based on the reviewed specification and stage-one behavior.

- [x] Implement the pending-first-approval and final-approval transitions for invoices above 500,000.
- [x] Require a distinct second approver who is a Director.
- [x] Reject duplicate-user attempts even when the same user has a Director role.
- [x] Validate a rejected attempt before changing invoice state or related approval records.
- [x] Add and review tests for valid two-person approval, duplicate users, and an invalid second approver.
- [x] Run stage-one tests again to confirm lower-value approvals still behave correctly.
- [x] Commit this stage separately with a message explaining the two-person rule.

**Exit evidence:** a second self-contained implementation commit and passing tests covering FR-03, FR-04, and FR-05. Technical evidence: [PHASE_5.md](docs/evidence/PHASE_5.md). 171 .NET and 46 Node cases pass; task 3.8 is complete after the participant answered `Reviewed; satisfied with the tests`. No Playwright or mutation completion is claimed.

## 6. Verify behavior and strengthen tests

### Minimum behavior matrix

Use the application's smallest supported monetary increment for values immediately below and above a threshold.

| Case | Expected result |
| --- | --- |
| Manager below and exactly at 50,000 | Approved. |
| Manager immediately above 50,000, within the single-approver range | Rejected with a clear reason; complete invoice state unchanged. |
| Director below and exactly at 500,000 | Approved. |
| Invoice immediately above 500,000, with one eligible approver | Pending second approval; not fully approved. |
| Invoice above 500,000, with two distinct eligible approvers and a Director second | Approved after the second approval. |
| Same user tries to count as both approvers | Rejected; state after the first approval remains unchanged. |
| Distinct second approver is not a Director | Rejected; state after the first approval remains unchanged. |
| Any other rejected approval path introduced by this feature | Clear reason; invoice and related approval state unchanged. |

- [ ] For rejection tests, compare the complete relevant state before and after the attempt, including persisted state where applicable. Check amounts, status, approver records, timestamps, and version fields that exist in the application.
- [ ] Add one Playwright test using an invoice just above the Manager limit but at or below the Director limit. Attempt approval as a Manager through the UI, verify the rejection message, and reload/check the persisted invoice to verify it did not change.
- [ ] Run mutation testing on the approval-rule code with the initial feature tests. Save this as the **before test-strengthening** result.
- [ ] Inspect surviving mutants and improve meaningful assertions or missing cases. Do not add tests that merely mirror the implementation.
- [ ] Rerun mutation testing with the same scope/configuration and unchanged production code where possible. Save this as the **after test-strengthening** result; explain any scope or code changes that affect comparability.
- [ ] Paste both scores as text in the write-up, along with commands, commit references, mutation counts, relevant configuration, and explanations for remaining survivors or exclusions.
- [ ] Run the unit suite and Playwright test successfully and commit the test improvements.

For a .NET 8 implementation, use the assignment's provided Stryker installation note in the requirements. Confirm runtime/tool compatibility during setup. The assignment does not specify a target mutation score; report actual results and improvements without inventing a threshold.

**Exit evidence:** reviewed tests, successful UI rejection test, comparable before/after mutation reports, and recorded human validation.

## 7. Open and review the pull request

- [ ] Push the assignment branch and open a pull request on the participant's fork.
- [ ] Obtain the exact workshop review prompt from workshop materials and run it against the PR diff. If it is unavailable, record that gap and obtain it before claiming this requirement is satisfied.
- [ ] Save the agent's review and give each finding an identifier.
- [ ] Have the participant judge each finding: **fixed**, **dismissed with a reason**, or **deferred with a reason**.
- [ ] Apply fixes, rerun the affected checks, and update the review record to reflect the final diff.
- [ ] Put every finding and its verdict in the PR description, with evidence or a reason.
- [ ] Confirm the PR contains at least three commits that stand on their own. Preserve the two separate implementation commits.

**Exit evidence:** PR URL, captured review prompt/output, complete finding dispositions, and passing checks for the final code.

## 8. Prepare the write-up and demo

- [ ] Create `ASSIGNMENT.md` at the application repository root.
- [ ] Explain the feature, specification decisions, context/skills/hooks, agent delegation, meaningful human interventions, and verification results.
- [ ] Include the PR URL, specification/test/log paths, mutation scores before and after as text, remaining limitations, and the final video link.
- [ ] Generate a readable log summary and verify the structured log against the nine suggested categories.
- [ ] Rehearse a live run with suitable seeded invoice data.
- [ ] Record an 8–10-minute MP4, upload it, and verify the submitted link is accessible to reviewers.

Suggested nine-minute recording:

| Time | Demonstration |
| --- | --- |
| 0:00–1:00 | Feature rules, repository, and key specification decisions. |
| 1:00–2:00 | `AGENTS.md`, selected skills/hooks, delegation, and a real interaction log. |
| 2:00–3:30 | Separate implementation commits and the resulting approval flow. |
| 3:30–5:30 | Run tests live, including the Playwright rejected-approval case. |
| 5:30–6:30 | Before/after mutation evidence and one meaningful test improvement. |
| 6:30–8:00 | PR review findings, participant verdicts, and resulting fixes. |
| 8:00–9:00 | Write-up, logs, and submission artifacts. |

**Exit evidence:** completed write-up, uploaded recording, and working reviewer links.

## 9. Commit and artifact plan

Suggested commit boundaries; adjust names and paths to the actual repository and generated tool layout.

| Commit | Scope |
| --- | --- |
| 1 | Context, logging hooks/skills, agent setup, and tool-generated specification/tasks. |
| 2 | Single-approver limits with reviewed tests. |
| 3 | Two-approver rule with reviewed tests. |
| 4 | Playwright coverage, test strengthening, and mutation evidence. |
| 5 | Review fixes, write-up, log summary, and demo link. Split fixes from documentation when appropriate. |

Keep logs current throughout the work rather than assembling them only in the final commit. Each implementation commit should pass the checks relevant to the behavior it introduces.

| Submission requirement | Planned artifact or evidence |
| --- | --- |
| Fork PR and at least three independent commits | PR URL and commit history. |
| Updated context | Root `AGENTS.md` and fresh-agent context check. |
| Spec-driven work | Actual generated specification/task files. |
| All approval rules tested | Unit tests mapped to FR-01–FR-05. |
| Rejected over-limit UI flow | Playwright test and successful execution result. |
| Mutation scores before and after | Saved reports and text in `ASSIGNMENT.md`. |
| Review judgments | Every finding and verdict in the PR description. |
| AI interaction record | Structured log, readable summary, and hook/skill configuration. |
| Root write-up | `ASSIGNMENT.md`. |
| Uploaded demo | Accessible 8–10-minute video link in the write-up. |

## 10. Final completion check

- [ ] Every item in the requirements submission checklist has a real artifact or link.
- [ ] Final tests correspond to the submitted production code; mutation evidence identifies the revisions tested.
- [ ] All review findings have participant verdicts, and fixed findings have been verified.
- [ ] The specification, behavior, tests, logs, PR description, and write-up agree.
- [ ] No sample log values are represented as execution evidence.
- [ ] The submitted PR preserves incremental commits, and the video plays for a reviewer.

Begin with Phase 1. Repository inspection determines the exact setup commands and test locations; resolve specification ambiguities before implementing the dependent approval transitions.
