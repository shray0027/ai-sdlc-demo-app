using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status) =>
        new() { Id = id, CustomerName = "Test School", Amount = 100m, Status = status };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Approve_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Approve("INV-1", "demo.approver", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.approver", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.approver", "Manager"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "demo.approver", "Manager"));
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Manager", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "Manager", "  "));
    }

    [Theory]
    [InlineData("Manager", "0.01")]
    [InlineData("Manager", "49999.99")]
    [InlineData("Manager", "50000.00")]
    [InlineData("Manager", "50000.000")]
    [InlineData("Director", "50000.01")]
    [InlineData("Director", "499999.99")]
    [InlineData("Director", "500000.00")]
    public void InclusiveLimits_SetCanonicalActorAndUtcDecision(string role, string amount)
    {
        var invoice = InvoiceState.Pending(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        invoice.DecidedBy = null; invoice.DecidedAt = null; invoice.DecisionNote = null;
        var repo = new FakeInvoiceRepository(invoice);
        var before = DateTime.UtcNow;
        var result = new ApprovalService(repo).Approve(invoice.Id, "canonical.user", role);
        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("canonical.user", result.DecidedBy);
        Assert.InRange(result.DecidedAt!.Value, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, result.DecidedAt.Value.Kind);
        Assert.Equal(invoice.Amount, result.Amount);
        Assert.Equal(invoice.CreatedAt, result.CreatedAt);
        Assert.Null(result.DecisionNote);
        Assert.Null(result.FirstApprovedBy); Assert.Null(result.FirstApprovedRole); Assert.Null(result.FirstApprovedAt);
        Assert.Equal(InvoiceState.Snapshot(result), InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }

    public static IEnumerable<object[]> FailedApprovals()
    {
        foreach (var role in new[] { "Admin", "Approver", "unknown", "", "manager" })
            yield return new object[] { 100m, InvoiceStatus.PendingApproval, "actor", role, typeof(UnauthorizedAccessException), "Managers and Directors" };
        foreach (var identity in new[] { "", "  " })
            yield return new object[] { 100m, InvoiceStatus.PendingApproval, identity, "Manager", typeof(ArgumentException), "identity" };
        foreach (var status in new[] { InvoiceStatus.Draft, InvoiceStatus.Approved, InvoiceStatus.Rejected })
            yield return new object[] { 100m, status, "actor", "Director", typeof(InvalidOperationException), "pending" };
        foreach (var amount in new[] { 0m, -0.01m, 0.001m, 50000.001m, 500000.001m })
            yield return new object[] { amount, InvoiceStatus.PendingApproval, "actor", "Director", typeof(ArgumentException), "whole cents" };
        foreach (var amount in new[] { 50000.01m, 500000m })
            yield return new object[] { amount, InvoiceStatus.PendingApproval, "actor", "Manager", typeof(InvalidOperationException), "50,000" };

    }

    [Theory]
    [MemberData(nameof(FailedApprovals))]
    public void EveryFailedApproval_LeavesFreshCompleteStateUnchanged(decimal amount, InvoiceStatus status, string identity, string role, Type exceptionType, string reason)
    {
        var invoice = InvoiceState.Pending(amount); invoice.Status = status;
        var repo = new FakeInvoiceRepository(invoice);
        var before = InvoiceState.Snapshot(repo.GetById(invoice.Id));
        var error = Record.Exception(() => new ApprovalService(repo).Approve(invoice.Id, identity, role));
        Assert.NotNull(error); Assert.IsType(exceptionType, error); Assert.Contains(reason, error.Message);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }

    [Fact]
    public void MissingInvoice_RemainsAbsent()
    {
        var repo = new FakeInvoiceRepository();
        Assert.Throws<KeyNotFoundException>(() => new ApprovalService(repo).Approve("missing", "actor", "Director"));
        Assert.Null(repo.GetById("missing")); Assert.Empty(repo.GetAll());
    }

    [Theory]
    [InlineData("Admin", "reason")]
    [InlineData("Approver", "reason")]
    [InlineData("unknown", "reason")]
    [InlineData("Manager", " ")]
    public void Reject_InvalidAuthorityOrReasonPreservesState(string role, string reason)
    {
        var invoice = InvoiceState.Pending(); var repo = new FakeInvoiceRepository(invoice);
        var before = InvoiceState.Snapshot(repo.GetById(invoice.Id));
        Assert.NotNull(Record.Exception(() => new ApprovalService(repo).Reject(invoice.Id, "actor", role, reason)));
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }
    [Theory]
    [InlineData("Manager", "500000.01")]
    [InlineData("Director", "500000.01")]
    [InlineData("Manager", "79228162514264337593543950335")]
    [InlineData("Director", "79228162514264337593543950335")]
    public void HighValue_FirstAndDistinctDirectorFinalPreserveCompleteHistory(string firstRole, string amount)
    {
        var expected = InvoiceState.HighValue(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        var repo = new FakeInvoiceRepository(expected); var service = new ApprovalService(repo);
        var started = DateTime.UtcNow;
        var first = service.Approve(expected.Id, "first.actor", firstRole);
        Assert.InRange(first.FirstApprovedAt!.Value, started, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, first.FirstApprovedAt.Value.Kind);
        expected.FirstApprovedBy = "first.actor"; expected.FirstApprovedRole = firstRole; expected.FirstApprovedAt = first.FirstApprovedAt;
        var pendingSnapshot = InvoiceState.Snapshot(expected);
        Assert.Equal(pendingSnapshot, InvoiceState.Snapshot(first));
        Assert.Equal(pendingSnapshot, InvoiceState.Snapshot(repo.GetById(expected.Id)));
        var final = service.Approve(expected.Id, "second.actor", "Director");
        Assert.InRange(final.DecidedAt!.Value, first.FirstApprovedAt.Value, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, final.DecidedAt.Value.Kind);
        expected.Status = InvoiceStatus.Approved; expected.DecidedBy = "second.actor"; expected.DecidedAt = final.DecidedAt;
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(final));
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(repo.GetById(expected.Id)));
        Assert.Equal(pendingSnapshot, InvoiceState.Snapshot(first));
    }

    [Theory]
    [InlineData("Director", "same", "Director", "distinct approver")]
    [InlineData("Manager", "same", "Director", "distinct approver")]
    [InlineData("Director", "same", "Manager", "distinct approver")]
    [InlineData("Manager", "same", "Manager", "distinct approver")]
    [InlineData("Director", "other", "Manager", "Director")]
    [InlineData("Manager", "other", "Manager", "Director")]
    public void HighValue_DuplicateBeforeWrongRoleAndDistinctManagerPreserveState(string firstRole, string secondIdentity, string secondRole, string reason)
    {
        var input = InvoiceState.HighValue(); InvoiceState.First(input, "same", firstRole);
        var repo = new FakeInvoiceRepository(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        var error = Assert.Throws<InvalidOperationException>(() => new ApprovalService(repo).Approve(input.Id, secondIdentity, secondRole));
        Assert.Contains(reason, error.Message); Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    public static IEnumerable<object[]> InvalidFirstMetadata()
    {
        yield return new object[] { "first", null!, null! };
        yield return new object[] { null!, "Manager", null! };
        yield return new object[] { null!, null!, DateTime.UtcNow };
        yield return new object[] { "first", "Director", null! };
        yield return new object[] { "first", null!, DateTime.UtcNow };
        yield return new object[] { null!, "Director", DateTime.UtcNow };
        yield return new object[] { " ", "Director", DateTime.UtcNow };
        yield return new object[] { "first", "Admin", DateTime.UtcNow };
        yield return new object[] { "first", "Director", default(DateTime) };
        yield return new object[] { "first", "Director", DateTime.SpecifyKind(new DateTime(2022, 1, 1), DateTimeKind.Unspecified) };
    }

    [Theory]
    [MemberData(nameof(InvalidFirstMetadata))]
    public void InvalidFirstMetadata_FailsClosedForApprovalAndRejection(string? identity, string? role, DateTime? at)
    {
        var input = InvoiceState.HighValue(); input.FirstApprovedBy = identity; input.FirstApprovedRole = role; input.FirstApprovedAt = at;
        var repo = new FakeInvoiceRepository(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        var service = new ApprovalService(repo);
        Assert.Contains("metadata", Assert.Throws<InvalidOperationException>(() => service.Approve(input.Id, "other", "Director")).Message);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        Assert.Contains("metadata", Assert.Throws<InvalidOperationException>(() => service.Reject(input.Id, "other", "Director", "intentional")).Message);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Theory]
    [InlineData("Manager", true)]
    [InlineData("Director", true)]
    [InlineData("Manager", false)]
    [InlineData("Director", false)]
    public void IntentionalReject_PreservesFirstApprovalIncludingSameActor(string role, bool sameActor)
    {
        var expected = InvoiceState.HighValue(); InvoiceState.First(expected);
        var repo = new FakeInvoiceRepository(expected); var actor = sameActor ? expected.FirstApprovedBy! : "other";
        var started = DateTime.UtcNow;
        var result = new ApprovalService(repo).Reject(expected.Id, actor, role, "Intentional rejection");
        Assert.InRange(result.DecidedAt!.Value, started, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, result.DecidedAt.Value.Kind);
        expected.Status = InvoiceStatus.Rejected; expected.DecidedBy = actor; expected.DecidedAt = result.DecidedAt; expected.DecisionNote = "Intentional rejection";
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(result));
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(repo.GetById(expected.Id)));
    }

    [Theory]
    [InlineData(InvoiceStatus.PendingApproval)]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Approved)]
    [InlineData(InvoiceStatus.Rejected)]
    public void FailuresWithFirstHistory_PreserveEveryField(InvoiceStatus status)
    {
        var input = InvoiceState.HighValue(); InvoiceState.First(input); input.Status = status;
        if (status != InvoiceStatus.PendingApproval) { input.DecidedBy = "final sentinel"; input.DecidedAt = DateTime.UtcNow; input.DecisionNote = "note"; }
        var repo = new FakeInvoiceRepository(input); var service = new ApprovalService(repo); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        Assert.Throws<ArgumentException>(() => service.Reject(input.Id, "first.actor", "Manager", " "));
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        if (status != InvoiceStatus.PendingApproval)
        {
            Assert.Contains("pending", Assert.Throws<InvalidOperationException>(() => service.Reject(input.Id, "other", "Director", "reason")).Message);
            Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
            Assert.Contains("pending", Assert.Throws<InvalidOperationException>(() => service.Approve(input.Id, "other", "Director")).Message);
            Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        }
    }

    [Fact]
    public void InconsistentTierOrFinalMetadata_FailsClosed()
    {
        foreach (var amount in new[] { 500000m, 500000.01m })
        {
            var input = InvoiceState.Pending(amount); InvoiceState.First(input);
            var repo = new FakeInvoiceRepository(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
            Assert.Contains("metadata", Assert.Throws<InvalidOperationException>(() => new ApprovalService(repo).Approve(input.Id, "other", "Director")).Message);
            Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
            Assert.Contains("metadata", Assert.Throws<InvalidOperationException>(() => new ApprovalService(repo).Reject(input.Id, "other", "Director", "reason")).Message);
            Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        }
    }
    [Theory]
    [InlineData("actor", "Admin", "500000.01", typeof(UnauthorizedAccessException), "Managers and Directors")]
    [InlineData("actor", "Approver", "500000.01", typeof(UnauthorizedAccessException), "Managers and Directors")]
    [InlineData("actor", "unknown", "500000.01", typeof(UnauthorizedAccessException), "Managers and Directors")]
    [InlineData(" ", "Director", "500000.01", typeof(ArgumentException), "identity")]
    [InlineData("actor", "Director", "500000.001", typeof(ArgumentException), "whole cents")]
    [InlineData("actor", "Director", "0", typeof(ArgumentException), "whole cents")]
    public void HighValueHistory_OtherFailedBranchesPreservePopulatedFirstFields(string actor, string role, string amount, Type type, string reason)
    {
        var input = InvoiceState.HighValue(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)); InvoiceState.First(input);
        var repo = new FakeInvoiceRepository(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        var error = Record.Exception(() => new ApprovalService(repo).Approve(input.Id, actor, role));
        Assert.IsType(type, error); Assert.Contains(reason, error.Message);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }
}
