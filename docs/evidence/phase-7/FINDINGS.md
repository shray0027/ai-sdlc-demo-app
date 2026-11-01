# Participant finding dispositions

These nine earlier findings were addressed and rechecked by agents. **Participant verdicts are pending**; overall test satisfaction does not supply them. Each requires Fixed, Dismissed with a reason, or Deferred with a reason. This ledger will also include every finding from the exact workshop review once its prompt is supplied.

| Finding | Agent handling/evidence | Participant verdict |
| --- | --- | --- |
| SPEC-01: distinguish temporary stage-one high-value tests from final stage-two expectations | Clarified/rechecked; `docs/agents/specification-review-dispositions.md` | Pending |
| S1-TR-01: reject JSON amounts before decimal conversion can round them | Exact converter and API regressions; `docs/agents/stage-one-test-review.md` | Pending |
| S1-TR-02: use an independent expected state for successful repository mutation | Complete independent oracle and recheck; same report | Pending |
| S1-TR-03: verify complete stable race responses | Full expected state/detached response assertions and recheck; same report | Pending |
| S1-C-01: preserve numeric form input before browser rounding | Lexical JSON-number transport and 42 focused Node regressions; `docs/agents/stage-one-coordinator-review.md` | Pending |
| ST2-T01: assert populated first-history preservation on actor/amount/terminal denials | Additional Domain/API matrices; `docs/agents/stage-two-test-review.md` | Pending |
| MI-01: non-draft submission explanation | Useful reason/full state across three statuses; `docs/agents/mutation-investigation-result.md` | Pending |
| MI-02: blank rejection-reason explanation | Useful explanation/full unchanged state; same report | Pending |
| MI-03: authenticated identity explanation | Meaningful wording rather than parameter suffix; same report | Pending |

The source reports under [docs/agents](../../agents/) preserve initial evidence and fixes. Full final checks passed on `0b91c4a`; see [final-checks.txt](final-checks.txt). No new production changes were needed to assemble this ledger.
