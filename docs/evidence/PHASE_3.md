# Phase 3 — specification and task breakdown

Status: complete. Participant choices are recorded; all four OpenSpec planning artifacts are generated, independently reviewed and strictly validated. Feature implementation remains pending.

## Tool selection and execution

Selected OpenSpec, one of the assignment's accepted spec-driven tools. Verified the [official installation](https://openspec.dev/docs/installation), [project setup](https://openspec.dev/docs/setup), and [CLI workflow](https://openspec.dev/docs/cli). The npm registry and installed CLI report version **1.16.0**.

Actual setup commands, reconstructed here from this conversation's tool results (automatic tool evidence is in the exported coordinator transcript):

```sh
npm view @fission-ai/openspec version engines --json
npm install --prefix .tools/openspec --save-exact @fission-ai/openspec@1.16.0
.tools/openspec/node_modules/.bin/openspec --version
.tools/openspec/node_modules/.bin/openspec init --tools codex --no-animation --profile core
```

The initial install ran under Node 22.12.0 and npm 11.1.0 and emitted engine warnings for transitive Inquirer packages, which require a newer compatible Node. Initialization and later commands used the already-installed **Node 24.18.0** by prepending its bin directory for those invocations; no global Node setting was changed. Future reproduction should select Node 24 before installation. Subsequent OpenSpec commands set `OPENSPEC_TELEMETRY=0`; initialization used its default setting.

Generated and preserved:

- `openspec/config.yaml` (then extended with assignment-specific constraints and artifact rules).
- `.agents/skills/.openspec-target` identifying Codex.
- Six generated skills: propose, apply-change, update-change, explore, sync-specs, and archive-change.

The coordinator actually read and applied `openspec-propose` plus the existing invoice-assignment skill. Other newly generated skills have not been invoked.

Project checks succeeded:

```sh
OPENSPEC_TELEMETRY=0 .tools/openspec/node_modules/.bin/openspec list --json
OPENSPEC_TELEMETRY=0 .tools/openspec/node_modules/.bin/openspec context --json
```

Saved results: [initial-list.json](openspec/initial-list.json), [initial-context.json](openspec/initial-context.json). Both resolve the application repository as the OpenSpec root. The initial list had no changes, before the generation step. Later artifact-generation evidence is recorded below.

## Discovery and participant clarification

The specification agent `/root/specification` inspected the actual application read-only and saved independent criteria and risks in [specification-discovery.md](../agents/specification-discovery.md). The exact delegation prompt is in [specification-discovery-prompt.txt](../agents/specification-discovery-prompt.txt).

[Policy decision record](../decisions/approval-policy.md) distinguishes observed behavior, proposed defaults, and pending human input. Two questions were submitted to the participant:

1. May a Manager or Director give the first approval above 500,000, or must both approvers be Directors? The distinct second Director requirement remains mandatory.
2. Should amounts be positive USD values with at most two decimal places (matching the UI), or should the API preserve its existing support for any positive decimal?

The participant explicitly replied `Manager or Director (recommended)` and `USD, at most two decimal places (recommended)`. These are actual answers, not inferred defaults. The decision record and OpenSpec context now reflect them.

The delegated specification agent then followed the actual OpenSpec proposal workflow through `new change`, `status`, and artifact-specific `instructions`, writing the artifacts in dependency order. Raw command outputs and the generation manifest are in [OpenSpec evidence](openspec/README.md). Implementation remains a separate staged workflow.


## Final artifacts and review

Change: `invoice-approval-limits`.

| Artifact | Result |
| --- | --- |
| [proposal.md](../../openspec/changes/invoice-approval-limits/proposal.md) | Scope, compatibility changes and capability definition |
| [spec.md](../../openspec/changes/invoice-approval-limits/specs/invoice-approval/spec.md) | FR-01–FR-05 plus named monetary, authority, progress, error and concurrency scenarios |
| [design.md](../../openspec/changes/invoice-approval-limits/design.md) | Domain policy, authenticated API identity, first-approval fields, copy-on-write repository boundary and UI integration |
| [tasks.md](../../openspec/changes/invoice-approval-limits/tasks.md) | Separate implementation prompts/commits and acceptance-to-test mapping; only completed planning review is checked |

The specification agent used invoice-assignment specification mode and the generated openspec-propose skill. The CLI created the scaffold and supplied artifact instructions/templates; the agent authored their content under that workflow. This is not a claim that a standalone CLI generated prose without agent involvement.

Independent reviewer `/root/spec_review` used invoice-assignment review mode and traced artifacts against the source and requirements. Its [initial review](../agents/specification-review.md) found one low-priority clarification, SPEC-01: explicitly replace temporary stage-one high-value-workflow-unavailable tests during stage two while retaining all lower-value regressions. The coordinator clarified tasks/design; the [narrow recheck](../agents/specification-review-recheck.md) confirms it addressed. [Agent handling](../agents/specification-review-dispositions.md) remains separate from any human verdict. Public JSON field casing was also aligned with the design without changing policy.

Final checks:

```sh
OPENSPEC_TELEMETRY=0 .tools/openspec/node_modules/.bin/openspec validate invoice-approval-limits --strict
OPENSPEC_TELEMETRY=0 .tools/openspec/node_modules/.bin/openspec status --change invoice-approval-limits --json
```

- Strict validation: **passed**, `Change 'invoice-approval-limits' is valid`. [Output](openspec/20-validation-submitted.txt).
- Planning status: **4/4 artifacts done**, dependencies preserved. [Final status](openspec/21-status-submitted.json). This is planning completion, not feature completion.
- Review: SPEC-01 addressed and independently rechecked; no blocking rule contradiction found.
- No production/test files changed, no application tests/mutations/browser flows executed in this phase, and no participant test/artifact/finding judgment inferred.

## Resolved specification details and boundaries

The participant selected Manager-or-Director first eligibility above USD 500,000 and positive numeric USD cents. The first approval is an explicit exception to final-approval limits and never makes the invoice Approved. Fractional-cent amounts are rejected, not rounded, at creation and Domain approval; trailing-zero scale does not invalidate a numeric cent value.

Agent-selected design uses canonical server-session username for distinctness; nullable firstApprovedBy/firstApprovedRole/firstApprovedAt while status remains PendingApproval; final decision fields unset until completion; Manager/Director approval and intentional-rejection roles without Admin bypass; shared-lock copy-on-write transitions and detached reads/results. Failed transitions commit no field or related-record changes. These choices are distinguished from human policy input in the decision record.

Stage one remains a self-contained single-approver implementation with a temporary high-value fail-closed response. Stage two supersedes only those temporary high-value expectations with the two-person workflow. All lower-value, role, precision, identity and immutability regressions remain. Playwright and comparable before/after mutation runs are planned later, including actual participant review.

The proposal skill's planning workflow ends with the artifacts; no apply workflow was invoked. The next assignment phase is the separate single-approver implementation task. Workshop PR review still requires its exact prompt and participant verdicts; neither is claimed by this specification review.

Logging note: the summary generator initially rejected a structured request value in the coordinator-authored metadata. The question/answer payload was normalized to JSON text, preserving both exact answer strings, structured human-intervention fields and the raw transcript. Summary generation then succeeded; no capture is represented as more complete than its provenance.
