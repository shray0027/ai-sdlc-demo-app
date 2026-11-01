# Phase 8 — write-up and recording handoff

Status: preparation complete; **recording/upload/share URL and playback/access verification pending**. The participant explicitly chose `I’ll record/upload and share the link`. No recording, duration, uploaded file or accessible link is invented.

## Completed preparation

- [ASSIGNMENT.md](../../ASSIGNMENT.md) explains rules/decisions, layering, context/skills/hooks and real capture limits, delegation, human judgments, verification, literal matching mutation scores and exact review evidence.
- [Submission checklist](../SUBMISSION_CHECKLIST.md) maps all eleven Section 8 checklist rows to artifacts. The root write-up/video-link row is partial and the uploaded-video row remains pending.
- [Nine-minute demo runbook](../DEMO_RUNBOOK.md) provides timed narration, seeded UI flows, actual live test commands and upload/access checks.
- [Log audit](phase-8/LOG_AUDIT.md) checks the nine suggested categories, JSONL structure and transcript provenance fields; [machine-readable audit](phase-8/log-audit.json). It distinguishes structured field presence from exhaustive semantic review and preserves the native IDE-hook limitation.

## Actual rehearsal

New [scripts/demo_rehearsal.py](../../scripts/demo_rehearsal.py) starts a separate owned loopback app using .NET 8, authenticates demo accounts without printing session tokens and uses real HTTP requests. Default mode verified both inclusive final limits, Manager over-limit rejection, Manager-first/Director-second, Director-first/distinct-Director-second, same Director after a fresh login, and distinct Manager second. Every denied attempt checks HTTP 409, a useful reason and full fresh 11-field persisted equality; successful high-value completion preserves first history. [Default output](phase-8/rehearsal.txt).

`--serve` repeated the live checks and seeded three pending UI invoices for USD 50,000, 50,000.01 and 500,000.01. The coordinator sent SIGINT after the seed-ready message and verified clean exit and closed port. [Seeded output](phase-8/serve-rehearsal.txt), [metadata](phase-8/rehearsal-metadata.json). Manual browser demonstration was not claimed for this API rehearsal; the existing Chromium UI evidence remains Phase 6/7.

Application/test/tool inputs still match all 47 Phase 7 hashes. [Input verification](phase-8/input-check.json) separately identifies the new helper hash, rather than claiming the prior review covered it. The helper is local demo tooling, outside production and test inputs. A [narrow independent follow-up review](../agents/phase-eight-demo-review.md), with its [saved exact prompt](../agents/phase-eight-demo-review-prompt.txt), reports no actionable new findings. It checked syntax, helper/runbook hashes, unchanged application hashes and both closed rehearsal ports. The report distinguishes coordinator execution from reviewer inspection and records limitations; no new participant verdict is inferred.

CI passed on starting head `f5694ef`: [remote metadata](phase-8/ci-starting-head.json). Later preparation pushes have their own CI status. No broad application/mutation rerun was needed for unchanged production/test inputs; default and seeded helper execution directly verify the new tooling.

## Participant next steps

Follow the runbook to record a readable 8–10-minute MP4 with live execution. Upload outside Git, enable reviewer access, check signed-out playback and share the real URL. Then update ASSIGNMENT.md and the PR with that URL, record duration/access verification and close the remaining checklist/plan items. No images/video are committed. PR #1 remains ready for review, unmerged; complete OpenSpec engineering tasks do not imply completed recording/submission.
