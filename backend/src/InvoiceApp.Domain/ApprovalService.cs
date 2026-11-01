namespace InvoiceApp.Domain;

/// <summary>Atomic invoice lifecycle and stage-one final approval limits.</summary>
public class ApprovalService(IInvoiceRepository repository)
{
    public Invoice Submit(string invoiceId) => repository.Mutate(invoiceId, invoice =>
    {
        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        invoice.Status = InvoiceStatus.PendingApproval;
    });

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        EnsureActor(approver, role);
        return repository.Mutate(invoiceId, invoice =>
        {
            EnsurePending(invoice);
            UsdAmount.Validate(invoice.Amount);
            if (invoice.Amount > 500000m)
                throw new InvalidOperationException("Two-person approval workflow is unavailable in this stage for amounts above USD 500,000.");
            if (role == "Manager" && invoice.Amount > 50000m)
                throw new InvalidOperationException("Manager approval limit is USD 50,000 inclusive; a Director is required.");
            invoice.Status = InvoiceStatus.Approved;
            invoice.DecidedBy = approver;
            invoice.DecidedAt = DateTime.UtcNow;
        });
    }

    public Invoice Reject(string invoiceId, string approver, string role, string reason)
    {
        EnsureActor(approver, role);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        return repository.Mutate(invoiceId, invoice =>
        {
            EnsurePending(invoice);
            invoice.Status = InvoiceStatus.Rejected;
            invoice.DecidedBy = approver;
            invoice.DecidedAt = DateTime.UtcNow;
            invoice.DecisionNote = reason;
        });
    }

    private static void EnsureActor(string identity, string role)
    {
        if (string.IsNullOrWhiteSpace(identity))
            throw new ArgumentException("An authenticated approver identity is required.", nameof(identity));
        if (role is not ("Manager" or "Director"))
            throw new UnauthorizedAccessException("Only Managers and Directors may approve or reject invoices.");
    }

    private static void EnsurePending(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
            throw new InvalidOperationException($"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
    }
}
