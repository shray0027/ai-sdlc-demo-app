# Stage-one coordinator integration review

Scope: approved stage-one Domain/API/repository and tests plus exact invoice-create transport. This is implementation review, not workshop PR review or participant judgment.

The coordinator inspected production changes and generated tests, requested actual-login Manager/Director endpoint success cases so incorrect controller-role wiring could not hide behind Domain tests, and independently passed the Release build and 98 .NET cases with no warnings.

## S1-C-01 — Browser Number conversion rounded before the API

Source: the creation submit handler previously used `Number(amount.value)`. Actual Node reproduction with raw `50000.0000000000000000000000001` produced JSON `{"amount":50000}`. Thus fixing server parsing alone could not recover precision already lost by the form.

The implementation follow-up preserved numeric text via lexical JSON-number normalization and safely encoded customerName separately; Domain/server retain positive-cent policy. The actual submit-handler tests cover exact threshold/max fractional tokens, ordinary cents, trailing zeros, exponent/syntax variants, invalid text preventing submission and customer escaping. The same independent child inspected the fix and passed 42 Node cases. Agent handling: addressed. Participant verdict: pending.

The design was clarified to require exact creation transport. The remaining large-number JavaScript limitation is display formatting of server-returned amounts, not input submission or stored/approved decimal values.

## Other review handling

S1-TR-01, S1-TR-02 and S1-TR-03 are recorded with initial evidence, fixes and final independent rechecks in `stage-one-test-review.md`: exact JSON-to-decimal conversion; independent successful mutation expected-state oracle; and full stable response assertions for races. All were addressed. No stage-two metadata/transitions, Playwright test or mutation run is included.

The new Microsoft.AspNetCore.Mvc.Testing 8.0.33 dependency was verified in the official NuGet flat-container version index and restored successfully, matching the installed ASP.NET Core runtime patch. CI now includes the dependency-free Node checks along with Release build/.NET tests. No remote CI run or participant test review is claimed.
