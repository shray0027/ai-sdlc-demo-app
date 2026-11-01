using InvoiceApp.Api;
using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class InMemoryInvoiceRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SnapshotContract_AllReferencesDetachedAndExceptionsDiscarded(bool fake)
    {
        IInvoiceRepository repo = fake ? new FakeInvoiceRepository() : new InMemoryInvoiceRepository();
        var input = InvoiceState.Pending(); InvoiceState.First(input); var expected = InvoiceState.Snapshot(input);
        var added = repo.Add(input);
        input.CustomerName = "input changed"; added.Amount = 333m;
        input.FirstApprovedBy = "changed input"; added.FirstApprovedRole = "changed add"; added.FirstApprovedAt = null;
        Assert.Equal(expected, InvoiceState.Snapshot(repo.GetById(input.Id)));
        var reads = new[] { repo.GetById(input.Id)!, repo.GetAll().Single(i => i.Id == input.Id), repo.GetByIds(new[] { input.Id }).Single() };
        foreach (var read in reads) { read.DecisionNote = "changed"; read.Status = InvoiceStatus.Rejected; read.FirstApprovedBy = null; read.FirstApprovedRole = null; read.FirstApprovedAt = null; }
        Assert.Equal(expected, InvoiceState.Snapshot(repo.GetById(input.Id)));
        Invoice? retained = null;
        var result = repo.Mutate(input.Id, working => { retained = working; working.CustomerName = "committed"; });
        var expectedCommitted = InvoiceState.Pending(); InvoiceState.First(expectedCommitted); expectedCommitted.CustomerName = "committed";
        var committed = InvoiceState.Snapshot(expectedCommitted);
        Assert.Equal(committed, InvoiceState.Snapshot(result));
        Assert.Equal(committed, InvoiceState.Snapshot(repo.GetById(input.Id)));
        retained!.CustomerName = "retained changed"; result.Amount = 444m;
        retained.FirstApprovedBy = "retained"; result.FirstApprovedRole = "result"; retained.FirstApprovedAt = null;
        Assert.Equal(committed, InvoiceState.Snapshot(repo.GetById(input.Id)));
        Assert.Throws<InvalidOperationException>(() => repo.Mutate(input.Id, working =>
        { working.FirstApprovedBy = "discard"; working.FirstApprovedRole = "Admin"; working.FirstApprovedAt = DateTime.UtcNow; working.Amount = 999m; working.DecidedAt = DateTime.UtcNow; working.DecisionNote = "discard"; throw new InvalidOperationException("deliberate"); }));
        Assert.Equal(committed, InvoiceState.Snapshot(repo.GetById(input.Id)));
        var invoked = false;
        Assert.Throws<KeyNotFoundException>(() => repo.Mutate("missing", _ => invoked = true));
        Assert.False(invoked); Assert.Null(repo.GetById("missing"));
    }

    private static async Task<Exception?[]> Race(Action first, Action second)
    {
        using var barrier = new Barrier(2);
        Task<Exception?> Run(Action action) => Task.Run<Exception?>(() => { Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10))); return Record.Exception(action); });
        return await Task.WhenAll(Run(first), Run(second));
    }

    [Fact]
    public async Task CompetingApprovals_ExactlyOneFinalDecisionAndStableResponse()
    {
        var repo = new InMemoryInvoiceRepository(); var invoice = repo.Add(InvoiceState.Pending());
        var first = new ApprovalService(repo); var second = new ApprovalService(repo);
        var startedAt = DateTime.UtcNow;
        Invoice? firstResult = null, secondResult = null;
        var errors = await Race(() => firstResult = first.Approve(invoice.Id, "director1", "Director"), () => secondResult = second.Approve(invoice.Id, "director2", "Director"));
        Assert.Single(errors, e => e == null); Assert.IsType<InvalidOperationException>(errors.Single(e => e != null));
        var winner = firstResult ?? secondResult!; var final = repo.GetById(invoice.Id)!;
        Assert.Equal(InvoiceState.Snapshot(winner), InvoiceState.Snapshot(final));
        Assert.Equal(InvoiceStatus.Approved, final.Status); Assert.Contains(final.DecidedBy, new[] { "director1", "director2" });
        Assert.InRange(final.DecidedAt!.Value, startedAt, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, final.DecidedAt.Value.Kind);
        invoice.Status = InvoiceStatus.Approved; invoice.DecidedBy = final.DecidedBy; invoice.DecidedAt = final.DecidedAt;
        Assert.Equal(InvoiceState.Snapshot(invoice), InvoiceState.Snapshot(final));
        winner.DecidedBy = "tampered"; Assert.Equal(final.DecidedBy, repo.GetById(invoice.Id)!.DecidedBy);
    }

    [Fact]
    public async Task SubmitAndApprove_OnlyValidSerialOutcomes()
    {
        var repo = new InMemoryInvoiceRepository(); var input = InvoiceState.Pending(); input.Status = InvoiceStatus.Draft;
        var invoice = repo.Add(input); var submitter = new ApprovalService(repo); var approver = new ApprovalService(repo);
        Invoice? submitted = null, approved = null;
        var startedAt = DateTime.UtcNow;
        var errors = await Race(() => submitted = submitter.Submit(invoice.Id), () => approved = approver.Approve(invoice.Id, "director", "Director"));
        input.Status = InvoiceStatus.PendingApproval;
        Assert.Equal(InvoiceState.Snapshot(input), InvoiceState.Snapshot(submitted));
        Assert.Null(errors[0]); var stored = repo.GetById(invoice.Id)!;
        if (errors[1] == null)
        {
            Assert.InRange(stored.DecidedAt!.Value, startedAt, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, stored.DecidedAt.Value.Kind);
            input.Status = InvoiceStatus.Approved; input.DecidedBy = "director"; input.DecidedAt = stored.DecidedAt;
            Assert.Equal(InvoiceState.Snapshot(input), InvoiceState.Snapshot(stored));
            Assert.Equal(InvoiceState.Snapshot(input), InvoiceState.Snapshot(approved));
        }
        else { Assert.IsType<InvalidOperationException>(errors[1]); Assert.Null(approved); Assert.Equal(InvoiceState.Snapshot(input), InvoiceState.Snapshot(stored)); }
    }

    [Fact]
    public async Task ApproveAndReject_ExactlyOneCoherentFinalDecision()
    {
        var repo = new InMemoryInvoiceRepository(); var invoice = InvoiceState.Pending(); invoice.DecidedBy = null; invoice.DecidedAt = null; invoice.DecisionNote = null; repo.Add(invoice);
        var approver = new ApprovalService(repo); var rejecter = new ApprovalService(repo);
        Invoice? approved = null, rejected = null;
        var startedAt = DateTime.UtcNow;
        var errors = await Race(() => approved = approver.Approve(invoice.Id, "director", "Director"), () => rejected = rejecter.Reject(invoice.Id, "manager", "Manager", "duplicate"));
        Assert.Single(errors, e => e == null); Assert.IsType<InvalidOperationException>(errors.Single(e => e != null));
        var stored = repo.GetById(invoice.Id)!; Assert.NotNull(stored.DecidedAt);
        if (stored.Status == InvoiceStatus.Approved) { Assert.Equal("director", stored.DecidedBy); Assert.Null(stored.DecisionNote); }
        else { Assert.Equal(InvoiceStatus.Rejected, stored.Status); Assert.Equal("manager", stored.DecidedBy); Assert.Equal("duplicate", stored.DecisionNote); }
        Assert.InRange(stored.DecidedAt!.Value, startedAt, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, stored.DecidedAt.Value.Kind);
        invoice.Status = errors[0] == null ? InvoiceStatus.Approved : InvoiceStatus.Rejected;
        invoice.DecidedBy = errors[0] == null ? "director" : "manager";
        invoice.DecisionNote = errors[0] == null ? null : "duplicate"; invoice.DecidedAt = stored.DecidedAt;
        Assert.Equal(InvoiceState.Snapshot(invoice), InvoiceState.Snapshot(stored));
        Assert.Equal(InvoiceState.Snapshot(invoice), InvoiceState.Snapshot(approved ?? rejected));
    }

    [Fact]
    public async Task ApproveAndDelete_NeverResurrectsInvoice()
    {
        var repo = new InMemoryInvoiceRepository(); var invoice = repo.Add(InvoiceState.Pending());
        var approver = new ApprovalService(repo);
        Invoice? result = null;
        var startedAt = DateTime.UtcNow;
        var errors = await Race(() => result = approver.Approve(invoice.Id, "director", "Director"), () => Assert.True(repo.Remove(invoice.Id)));
        Assert.Null(errors[1]);
        if (errors[0] != null) { Assert.IsType<KeyNotFoundException>(errors[0]); Assert.Null(result); }
        else
        {
            Assert.NotNull(result); Assert.NotNull(result.DecidedAt);
            Assert.InRange(result.DecidedAt.Value, startedAt, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, result.DecidedAt.Value.Kind);
            invoice.Status = InvoiceStatus.Approved; invoice.DecidedBy = "director"; invoice.DecidedAt = result.DecidedAt;
            Assert.Equal(InvoiceState.Snapshot(invoice), InvoiceState.Snapshot(result));
        }
        Assert.Null(repo.GetById(invoice.Id)); Assert.Empty(repo.GetByIds(new[] { invoice.Id }));
    }
    private static void AssertFirstResponse(Invoice baseline, Invoice response, string actor, string role, DateTime started)
    {
        Assert.NotNull(response.FirstApprovedAt); Assert.InRange(response.FirstApprovedAt.Value, started, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, response.FirstApprovedAt.Value.Kind);
        // Build expectations from the original fixture, not production Snapshot/copying.
        var expected = InvoiceState.HighValue(baseline.Amount);
        expected.FirstApprovedBy = actor; expected.FirstApprovedRole = role; expected.FirstApprovedAt = response.FirstApprovedAt;
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(response));
    }

    private static void AssertFinalResponse(Invoice first, Invoice response, string actor, InvoiceStatus status, string? note, DateTime started)
    {
        Assert.NotNull(response.DecidedAt); Assert.InRange(response.DecidedAt.Value, started, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, response.DecidedAt.Value.Kind);
        var expected = InvoiceState.HighValue(first.Amount);
        expected.FirstApprovedBy = first.FirstApprovedBy; expected.FirstApprovedRole = first.FirstApprovedRole; expected.FirstApprovedAt = first.FirstApprovedAt;
        expected.Status = status; expected.DecidedBy = actor; expected.DecidedAt = response.DecidedAt; expected.DecisionNote = note;
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(response));
    }

    [Theory]
    [InlineData("Director", "same", "Director", "same")]
    [InlineData("Director", "director1", "Director", "director2")]
    [InlineData("Manager", "manager", "Director", "director")]
    [InlineData("Director", "director", "Manager", "manager")]
    public async Task HighValue_ConcurrentFirstApprovalsHaveOnlySerialOutcomes(string role1, string actor1, string role2, string actor2)
    {
        var repo = new InMemoryInvoiceRepository(); var input = repo.Add(InvoiceState.HighValue());
        var service1 = new ApprovalService(repo); var service2 = new ApprovalService(repo);
        Invoice? result1 = null, result2 = null; var started = DateTime.UtcNow;
        var errors = await Race(() => result1 = service1.Approve(input.Id, actor1, role1), () => result2 = service2.Approve(input.Id, actor2, role2));
        var results = new[] { result1, result2 }; var actors = new[] { actor1, actor2 }; var roles = new[] { role1, role2 };
        var firstIndex = Array.FindIndex(results, r => r?.Status == InvoiceStatus.PendingApproval); Assert.InRange(firstIndex, 0, 1);
        var first = results[firstIndex]!; var secondIndex = 1 - firstIndex;
        AssertFirstResponse(input, first, actors[firstIndex], roles[firstIndex], started);
        var pendingSnapshot = InvoiceState.Snapshot(first); var stored = repo.GetById(input.Id)!;
        if (actor1 == actor2 || roles[secondIndex] != "Director")
        {
            Assert.Null(errors[firstIndex]); Assert.Null(results[secondIndex]);
            var error = Assert.IsType<InvalidOperationException>(errors[secondIndex]);
            Assert.Contains(actor1 == actor2 ? "distinct approver" : "Director", error.Message);
            Assert.Equal(pendingSnapshot, InvoiceState.Snapshot(stored));
        }
        else
        {
            Assert.All(errors, error => Assert.Null(error));
            AssertFinalResponse(first, results[secondIndex]!, actors[secondIndex], InvoiceStatus.Approved, null, started);
            Assert.Equal(InvoiceState.Snapshot(results[secondIndex]), InvoiceState.Snapshot(stored));
        }
        Assert.Equal(pendingSnapshot, InvoiceState.Snapshot(first));
        first.FirstApprovedBy = "tampered"; first.FirstApprovedRole = "Admin"; first.FirstApprovedAt = null;
        Assert.Equal(InvoiceState.Snapshot(stored), InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Fact]
    public async Task HighValue_CompetingFinalDirectorsCannotOverwriteFirstOrFinalHistory()
    {
        var repo = new InMemoryInvoiceRepository(); var input = InvoiceState.HighValue(); InvoiceState.First(input); repo.Add(input);
        var service1 = new ApprovalService(repo); var service2 = new ApprovalService(repo);
        Invoice? result1 = null, result2 = null; var started = DateTime.UtcNow;
        var errors = await Race(() => result1 = service1.Approve(input.Id, "director1", "Director"), () => result2 = service2.Approve(input.Id, "director2", "Director"));
        Assert.Single(errors, e => e == null); Assert.Contains("pending", Assert.IsType<InvalidOperationException>(errors.Single(e => e != null)).Message);
        var winner = result1 ?? result2!;
        AssertFinalResponse(input, winner, result1 != null ? "director1" : "director2", InvoiceStatus.Approved, null, started);
        Assert.Equal(InvoiceState.Snapshot(winner), InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HighValue_ApproveRejectRacePreservesCoherentHistory(bool hasFirst)
    {
        var repo = new InMemoryInvoiceRepository(); var input = InvoiceState.HighValue(); if (hasFirst) InvoiceState.First(input); repo.Add(input);
        var approver = new ApprovalService(repo); var rejecter = new ApprovalService(repo);
        Invoice? approved = null, rejected = null; var started = DateTime.UtcNow;
        var errors = await Race(() => approved = approver.Approve(input.Id, "director", "Director"), () => rejected = rejecter.Reject(input.Id, "manager", "Manager", "intentional"));
        var stored = repo.GetById(input.Id)!;
        if (hasFirst)
        {
            Assert.Single(errors, e => e == null); Assert.Contains("pending", Assert.IsType<InvalidOperationException>(errors.Single(e => e != null)).Message);
            var winner = approved ?? rejected!;
            AssertFinalResponse(input, winner, approved != null ? "director" : "manager", approved != null ? InvoiceStatus.Approved : InvoiceStatus.Rejected, approved != null ? null : "intentional", started);
            Assert.Equal(InvoiceState.Snapshot(winner), InvoiceState.Snapshot(stored));
        }
        else if (approved != null)
        {
            Assert.All(errors, e => Assert.Null(e)); AssertFirstResponse(input, approved, "director", "Director", started);
            AssertFinalResponse(approved, rejected!, "manager", InvoiceStatus.Rejected, "intentional", started);
            Assert.Equal(InvoiceState.Snapshot(rejected), InvoiceState.Snapshot(stored));
        }
        else
        {
            Assert.Null(errors[1]); Assert.Contains("pending", Assert.IsType<InvalidOperationException>(errors[0]).Message);
            AssertFinalResponse(input, rejected!, "manager", InvoiceStatus.Rejected, "intentional", started);
            Assert.Equal(InvoiceState.Snapshot(rejected), InvoiceState.Snapshot(stored));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HighValue_ApproveDeleteRaceNeverResurrectsOrChangesResponse(bool hasFirst)
    {
        var repo = new InMemoryInvoiceRepository(); var input = InvoiceState.HighValue(); if (hasFirst) InvoiceState.First(input); repo.Add(input);
        var service = new ApprovalService(repo); Invoice? response = null; var started = DateTime.UtcNow;
        var errors = await Race(() => response = service.Approve(input.Id, "director", "Director"), () => Assert.True(repo.Remove(input.Id)));
        Assert.Null(errors[1]);
        if (errors[0] != null) { Assert.IsType<KeyNotFoundException>(errors[0]); Assert.Null(response); }
        else if (hasFirst) AssertFinalResponse(input, response!, "director", InvoiceStatus.Approved, null, started);
        else AssertFirstResponse(input, response!, "director", "Director", started);
        Assert.Null(repo.GetById(input.Id)); Assert.Empty(repo.GetByIds(new[] { input.Id }));
    }
}
