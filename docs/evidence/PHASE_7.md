# Phase 7 — draft PR and review preparation

Status: in progress. [Draft PR #1](https://github.com/shray0027/ai-sdlc-demo-app/pull/1) targets main on the participant fork. No merge is requested or performed.

The branch was pushed after verifying GitHub authentication, the unchanged remote main base `66cb3f9005af1feafb7890f06bbc41fd652b432a`, no existing matching PR and no tracked images. The initial PR head was `0b91c4acc18a641be463b8803b0eee4d78c4d7f3`; [opening metadata](phase-7/pr-opened.json) preserves all nine original commits, including separate implementations `5cf4d61` and `df5fa14`. This phase adds evidence-only changes.

## Final integration checks

All [27 named OpenSpec scenarios](phase-7/ACCEPTANCE_MAP.md) link to concrete test methods/cases. [Final commands/output](phase-7/final-checks.txt): strict OpenSpec validation passed, Release build zero warnings/errors, 173 .NET tests, 46 JavaScript tests and one Chromium test passed on the initial PR head. The new [browser state](phase-7/invoice-state.json) records the complete unchanged 11-field invoice after HTTP 409 and reload. Phase 6 evidence bytes were preserved; the browser runner used list-only reporting and the generated state was moved here with the original restored.

Task 5.1 was checked only after verifying its [exact source line](phase-7/task-source-verification.json); [refreshed apply state](phase-7/openspec-apply.json) shows 24/26 complete. Tasks 5.2/5.3 remain open. No mutation rerun was needed for documentation-only changes; the fixed production/test experiment and both scores remain the Phase 6 evidence.

## Pending exact workshop review and participant judgments

Requirements section 6 explicitly requires the workshop review prompt against the diff and participant judgment of every finding. The repository contains that instruction but not the prompt text. The coordinator requested the exact prompt or a local path and continues independent PR/evidence work while waiting. No replacement prompt is presented as workshop review, and no PR review agent has been invoked yet.

[Nine earlier findings and concrete fixes](phase-7/FINDINGS.md) are included in the PR description with participant verdicts pending. Earlier generated-test satisfaction replies are not reused as finding verdicts. The participant was asked to review these fixes and provide Fixed, Dismissed with reason or Deferred with reason for each. Any workshop findings will be appended. Production changes, test reruns for new fixes and completion of review remain dependent on actual findings/answers.

Remote GitHub CI passed on the initial PR head: [captured check metadata](phase-7/ci-initial-head.json). This is separate evidence from the local checks. The final evidence push may trigger another run. PR remains draft until the required review work is complete. The uploaded demo is still Phase 8 work.
