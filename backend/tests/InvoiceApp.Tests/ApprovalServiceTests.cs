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
        foreach (var role in new[] { "Manager", "Director" })
            foreach (var amount in new[] { 500000.01m, decimal.MaxValue })
                yield return new object[] { amount, InvoiceStatus.PendingApproval, "actor", role, typeof(InvalidOperationException), "Two-person" };
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
}
