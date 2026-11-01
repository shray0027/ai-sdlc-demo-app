# Mutation test investigation

Parent task `/root/phase_six_mutation`, coordinator `/root`, child `/root/phase_six_mutation/mutation_test_review`. Exact prompt: `mutation-investigation-prompt.txt`; spawned message directed child to that saved prompt, with no builds without coordination. Follow-up supplied timeout IDs 3/42/91 and requested useful error-reason assessment. Child performed no builds or edits.

## Exact child final result

Read-only review completed by `/root/phase_six_mutation/mutation_test_review`, parent `/root/phase_six_mutation`.

BEFORE report confirms **100.00%: 97 Killed, 3 Timeout, 35 Ignored, zero Survived/NoCoverage**. Ignored reasons are 24 outside explicit scope and 11 standard block-covered filters.

Three meaningful assertion gaps merit strengthening despite no survivors:

- **MI-01 — id 3, Submit error:** `Submit_RejectsInvoiceThatIsNotADraft` checks exception type only. Assert a useful “only drafts” explanation and complete fresh persisted-state equality.
- **MI-02 — id 42, blank rejection reason:** Existing tests check exception type/state, without requiring an explanation. Assert “reason is required” and retain full-state equality.
- **MI-03 — id 91, blank identity:** Existing `Contains("identity")` can match `ArgumentException`’s parameter-name suffix even if its explanatory message is empty. Require meaningful wording such as “authenticated approver identity is required”, including the corresponding API cases where practical. A simple nonempty assertion would remain insufficient.

These are **timeout mutants**, not established survivors or equivalent mutants. Empty strings introduce no loop; their timeouts do not demonstrate assertion detection. Keep counts truthful and assess whether AFTER converts these to Killed without claiming score improvement.

Existing tests already cover inclusive/adjacent thresholds, same Director and role-changed duplicate identities, distinct Manager denial, valid two-person completion, metadata invariants, complete serialized snapshots and fresh repository reads. No additional threshold/duplicate/state cases are justified by observed survivors.

Used `invoice-assignment` testing/review guidance; read AGENTS.md, relevant ACTION_PLAN guidance, Domain code, tests, config and preserved JSON/console reports. Tools: read-only shell inspection and JSON analysis. No edits, test execution, builds, browser activity or production changes. Participant review remains pending.

## Parent handling

MI-01/02/03 accepted for test-only strengthening after untouched BEFORE preservation. Submission failure checks now assert useful reason and complete fresh state for PendingApproval/Approved/Rejected. Blank reason asserts the useful explanation plus complete state; low-value invalid-identity cases assert meaningful explanation rather than a parameter suffix. Existing tests remain otherwise intact; exact before/after diff is preserved under mutation evidence. API message strengthening was not required to demonstrate the Domain contract gap: the same service text flows to API, while direct Domain checks isolate the explanation independently of transport. Participant judgment remains pending.
