# AI in SDLC Assignment — Requirements

This document consolidates the assignment and follow-up logging guidance shown in the eight supplied images. It captures the feature, development process, testing, review, demo, and submission requirements. Recommendations in the follow-up guidance are identified as preferences rather than mandatory requirements.

## 1. Objective

Take one feature from a written specification through to a reviewed pull request on the workshop demo repository. Use an AI agent to perform the work while retaining responsibility for checks and decisions.

The feature is deliberately small. The assignment emphasizes evidence around the implementation:

- A context file that enables an agent to work effectively.
- Tests that would catch real mistakes.
- A judgment on every AI review finding.

## 2. Repository and Tooling

- Fork the workshop demo repository: [ai-sdlc-demo-app](https://github.com/NITIN1670/ai-sdlc-demo-app.git).
- There is no programming-language constraint; any language may be used.
- The assignment is not tied to one AI vendor. The source suggests the free tier of GitHub Copilot over a paid AI tool; alternatives are welcome provided they meet compliance requirements.
- Use a spec-driven tool such as Spec Kit, OpenSpec, or BMAD to generate the specification and task breakdown.
- Use appropriate agent skills and hooks.
- Create multiple agents and sub-agents for the assignment.

## 3. Feature: Invoice Approval Limits by Role

Add role-based approval limits to the invoice approval service in `ai-sdlc-demo-app`.

| ID | Requirement |
| --- | --- |
| FR-01 | A Manager may approve invoices up to and including 50,000. |
| FR-02 | A Director may approve invoices up to and including 500,000. |
| FR-03 | An invoice above 500,000 requires two distinct approvers, and the second approver must be a Director. |
| FR-04 | The same user must never count as both approvers on one invoice. |
| FR-05 | A rejected approval attempt must return a clear reason and must leave everything about the invoice unchanged. |

The source particularly emphasizes FR-04 and FR-05 as rules that can appear correct in generated code while still being implemented incorrectly. Plan tests specifically around them.

### Details not specified in the source

The images do not specify the currency, the eligible role of the first approver for invoices above 500,000, or how a pending first approval is represented. Resolve these details in the specification without weakening the explicit rules above.

## 4. Context and Agent Workflow

### 4.1 Repository context

Extend `AGENTS.md` so an agent arriving without prior context understands:

- The approval rules.
- The application's layering.
- The command used to run tests.
- The review expectations.

Show that the context matters to the agent's work.

### 4.2 Skills, hooks, and interaction logs

- Use appropriate skills for the agents.
- Use appropriate hooks to generate structured output in a consistent format.
- Log all AI interactions in a log file using skills and/or hooks.
- Use multiple agents and sub-agents.

#### AI augmentation log guidance

The follow-up guidance says logs should ideally capture the following details:

| Detail | Information to capture |
| --- | --- |
| AI tool and model | The tool and model used, such as GitHub Copilot, Claude, or Cursor. |
| Agent details | Agent type (built-in, custom, or sub-agent) and execution mode (Ask, Plan, Agent, or Edit). |
| Prompts and interactions | Actual queries or prompts submitted to the agent, including follow-up interactions. |
| Skills and sub-agents | Skills invoked, custom instructions followed, and sub-agents used. |
| Tools and integrations | MCP servers, external tools, and other capabilities accessed. |
| AI actions and outputs | Tasks performed, code generated or modified, tests created, and other deliverables. |
| Human intervention | Manual changes, decisions, corrections, and validation performed. |
| Execution evidence | Timestamps, affected files, test results, and relevant session or commit references. |
| Outcome | Whether the interaction succeeded, was partially completed, or required rework. |

The objective is traceability across the AI-assisted development lifecycle: show how effectively AI was used, which capabilities were used, and how the participant validated and refined the generated output. The focus is on quality of AI use, transparency, and evidence of human oversight rather than simply the number of prompts or agents.

- Ideally generate logs automatically wherever the tool supports it, rather than relying entirely on manual documentation.
- A structured JSON or JSONL log with a readable summary is preferred.
- The supplied JSON is a reference format, not a fixed schema; additional useful details may be added.

#### Reference JSON format from the follow-up images

The example below transcribes the supplied sample. Its timestamp, task, status, and artifact path are illustrative source values, not evidence of work performed in this repository. Extend the sample as needed to capture all nine categories above, including test results, human changes, and outcomes.

```json
{
  "id": "20261019T063542Z-agent-requirement-transcription",
  "timestamp": "2026-10-19T06:35:42Z",
  "stage": "setup",
  "agent": "agent",
  "agent_name": "<agent_name>",
  "agent_type": "<built-in | custom | sub-agent>",
  "ai_tool": "<ai_tool_name>",
  "model": "<model_name>",
  "mode": "<ask | plan | agent | edit>",
  "related_ids": [],
  "parent_task": null,
  "context": "Assignment for AI in SDLC",
  "request": "Analyse Assignment requirement",
  "skills_used": [
    {
      "name": "<skill_name>",
      "type": "<built-in | custom>",
      "purpose": "<skill_purpose>",
      "invocation": "<automatic | explicit>"
    }
  ],
  "subagents_used": [
    {
      "name": "<subagent_name>",
      "type": "<built-in | custom>",
      "task": "<delegated_task>",
      "status": "<completed | failed | partial>"
    }
  ],
  "mcp_tools_used": [
    {
      "server": "<mcp_server_name>",
      "tool": "<mcp_tool_name>",
      "purpose": "<purpose>"
    }
  ],
  "tools_used": [
    "<tool_name>"
  ],
  "suggestion": "<Any suggestions>",
  "human_decision": null,
  "action_verification": "<action verification>",
  "limitation": null,
  "status": "applied",
  "artifacts": [
    "requirement/complete-requirement.md"
  ]
}
```

### 4.3 Specification

- Use Spec Kit, OpenSpec, BMAD, or another selected spec-driven tool to produce the specification and task breakdown.
- Commit the files generated by that tool.

### 4.4 Incremental implementation

- Implement the feature in at least two steps; do not request the whole feature in a single prompt.
- First implement the single-approver limits with tests.
- Then implement the two-approver rule.
- Give each step its own commit, with a message explaining what changed and why.
- The submitted pull request must contain at least three commits that each stand on their own.

## 5. Testing and Mutation Evidence

- Write unit tests for every approval rule, including the two negative rules: duplicate approvers are forbidden, and rejected attempts must not change the invoice.
- Review every generated test.
- Add one Playwright end-to-end test that exercises a rejected over-limit approval through the user interface.
- Record mutation scores before and after, and paste those scores as text into the write-up.

### Source installation note for .NET 8

The assignment specifies Stryker version `4.13.0` for mutation testing on .NET 8:

```sh
dotnet tool install -g dotnet-stryker --version 4.13.0
```

The source warns that the newest release targets a later runtime and will fail to install on .NET 8. This is an assignment-provided note, not a current package-version verification.

## 6. Pull Request Review

- Open a pull request on your fork.
- Run the workshop review prompt against the diff using the agent.
- Judge the agent's review yourself.
- List every review finding in the pull request description with one of these verdicts:
  - **Fixed**.
  - **Dismissed**, with a reason.
  - **Deferred**, with a reason.

## 7. Demo Video

- Record one demo video lasting 8–10 minutes.
- Show something running live on screen, such as a test, a mutation run, or the end-to-end test.
- A read-through of files alone does not count as a demo.
- Record using Teams by starting a meeting with only yourself and pressing Record, or use any screen recorder that produces an MP4.
- Upload the video and include its link in the submission.

## 8. Submission Checklist

- [ ] A pull request on your fork containing at least three commits that each stand on their own.
- [ ] The updated `AGENTS.md`.
- [ ] The specification files from the selected spec-driven tool.
- [ ] Unit tests for every approval rule, including the two negative rules.
- [ ] One Playwright test covering a rejected over-limit approval.
- [ ] Mutation scores before and after, pasted as text in the write-up.
- [ ] A pull request description listing every review finding with a verdict.
- [ ] A log file capturing all AI interactions.
- [ ] Check the log against the nine suggested detail categories in Section 4.2; prefer automatic capture and JSON/JSONL with a readable summary where supported.
- [ ] `ASSIGNMENT.md` at the repository root containing the write-up and video link.
- [ ] The uploaded and linked 8–10-minute demo video.

## 9. Source Images

- [WhatsApp Image 2026-11-01 at 13.28.58.jpeg](<WhatsApp Image 2026-11-01 at 13.28.58.jpeg>) — assignment overview, feature rules, and workflow.
- [WhatsApp Image 2026-11-01 at 13.28.57.jpeg](<WhatsApp Image 2026-11-01 at 13.28.57.jpeg>) — workflow continuation, demo requirements, and submission checklist.
- [WhatsApp Image 2026-11-01 at 13.51.01.jpeg](<WhatsApp Image 2026-11-01 at 13.51.01.jpeg>) — logging clarification, AI tool/model, and agent details.
- [WhatsApp Image 2026-11-01 at 13.51.07.jpeg](<WhatsApp Image 2026-11-01 at 13.51.07.jpeg>) — prompts, skills, integrations, actions, human intervention, and execution evidence.
- [WhatsApp Image 2026-11-01 at 13.51.28.jpeg](<WhatsApp Image 2026-11-01 at 13.51.28.jpeg>) — outcomes, traceability, automatic logging, and preferred format.
- [WhatsApp Image 2026-11-01 at 13.51.34.jpeg](<WhatsApp Image 2026-11-01 at 13.51.34.jpeg>) — quality, transparency, human oversight, and reference-format flexibility.
- [WhatsApp Image 2026-11-01 at 13.51.41.jpeg](<WhatsApp Image 2026-11-01 at 13.51.41.jpeg>) — reference JSON, first portion.
- [WhatsApp Image 2026-11-01 at 13.51.47.jpeg](<WhatsApp Image 2026-11-01 at 13.51.47.jpeg>) — reference JSON, remaining portion.
