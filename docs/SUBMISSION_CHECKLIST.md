# Submission handoff checklist

Engineering and review are complete. Final submission is **pending the participant's recording, upload, and video link**. PR: https://github.com/shray0027/ai-sdlc-demo-app/pull/1. It is ready for review; no merge or human GitHub approval is claimed.

| Requirement (requirements/REQUIREMENTS.md section 8) | Status | Artifact |
| --- | --- | --- |
| Fork PR with at least three independent commits | Complete | PR #1; separate `5cf4d61` and `df5fa14`, retained context/spec/test/evidence stages |
| Updated agent context | Complete | [AGENTS.md](../AGENTS.md); [fresh-agent check](agents/context-check-response.md) |
| Tool-generated specification/task files | Complete | [OpenSpec change](../openspec/changes/invoice-approval-limits/); [CLI evidence](evidence/openspec/README.md) |
| Unit tests for every approval rule and negative rules | Complete | [27-scenario map](evidence/phase-7/ACCEPTANCE_MAP.md); [actual checks](evidence/phase-7/final-checks.txt) |
| Playwright over-limit UI rejection | Complete | [Test](../e2e/tests/manager-limit.spec.ts); [browser evidence](evidence/phase-6/playwright/README.md) |
| Before/after mutation scores as text | Complete | [ASSIGNMENT.md](../ASSIGNMENT.md); [same-scope raw reports](evidence/phase-6/mutation/README.md) |
| Every review finding with participant verdict in PR | Complete | [Nine Fixed verdicts](evidence/phase-7/FINDINGS.md); [exact workshop review](agents/workshop-review.md), no new findings |
| Interaction log | Complete with capture limits | [Structured records](../logs/interactions.jsonl); [automatic transcript exports](../logs/transcripts/); [coverage/activation limits](../logs/README.md) |
| Nine suggested log categories checked | Complete | [Audit](evidence/phase-8/LOG_AUDIT.md); [readable summary](../logs/SUMMARY.md) |
| Root write-up and video link | **Partial: write-up complete, video link missing** | [ASSIGNMENT.md](../ASSIGNMENT.md) |
| Uploaded accessible 8–10-minute MP4 | **Pending participant** | [Nine-minute demo runbook](DEMO_RUNBOOK.md) |

## Final checks before delivery

- Application/test/tool inputs match the already tested and independently reviewed revision; Phase 8 adds a demo helper and handoff evidence. [Input check](evidence/phase-8/input-check.json).
- CI passed on Phase 8 starting head `f5694ef`; [exact remote metadata](evidence/phase-8/ci-starting-head.json). Later pushes have their own CI status.
- Demo helper's real API rehearsal and seeded UI mode passed with clean owned-server teardown; [rehearsal output](evidence/phase-8/rehearsal.txt), [seeded-mode output](evidence/phase-8/serve-rehearsal.txt), [metadata](evidence/phase-8/rehearsal-metadata.json). These are live execution evidence, not a recorded video or proof of manual UI playback.
- All actual findings have explicit participant verdicts; no sample record is represented as execution. Native IDE hook activation is still unverified and labeled.
- No source screenshots, app screenshots, video, tokens or runtime artifacts are added to Git. Upload the recording outside the repository.
- After upload: supply the share URL, verify 8–10-minute duration and readable live execution, and check signed-out reviewer access. Then add the URL to ASSIGNMENT.md/PR, update this checklist and complete the action plan's remaining items.

The participant explicitly chose `I’ll record/upload and share the link`. The agent will finish the link handoff once that real link is supplied; no dummy link or recording-complete claim is used.
