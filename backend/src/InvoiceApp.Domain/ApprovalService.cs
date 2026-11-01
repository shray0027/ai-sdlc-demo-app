namespace InvoiceApp.Domain;

/// <summary>Atomic invoice lifecycle with role limits and distinct-person approvals.</summary>
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
            EnsureFirstApprovalMetadata(invoice);
            if (invoice.Amount > 500000m)
            {
                if (invoice.FirstApprovedBy == null)
                {
                    invoice.FirstApprovedBy = approver;
                    invoice.FirstApprovedRole = role;
                    invoice.FirstApprovedAt = DateTime.UtcNow;
                    return;
                }
                if (string.Equals(invoice.FirstApprovedBy, approver, StringComparison.Ordinal))
                    throw new InvalidOperationException("A distinct approver is required; the same user cannot approve twice.");
                if (role != "Director")
                    throw new InvalidOperationException("A distinct Director is required for the second approval.");
            }
            if (invoice.Amount <= 500000m && role == "Manager" && invoice.Amount > 50000m)
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
            EnsureFirstApprovalMetadata(invoice);
            invoice.Status = InvoiceStatus.Rejected;
            invoice.DecidedBy = approver;
            invoice.DecidedAt = DateTime.UtcNow;
            invoice.DecisionNote = reason;
        });
    }

    private static void EnsureFirstApprovalMetadata(Invoice invoice)
    {
        if (invoice.Amount > 500000m && (invoice.DecidedBy != null || invoice.DecidedAt != null || invoice.DecisionNote != null))
            throw new InvalidOperationException("Pending high-value invoice has invalid final decision metadata.");
        if (invoice.FirstApprovedBy == null && invoice.FirstApprovedRole == null && invoice.FirstApprovedAt == null)
            return;
        if (string.IsNullOrWhiteSpace(invoice.FirstApprovedBy) ||
            invoice.FirstApprovedRole is not ("Manager" or "Director") ||
            invoice.FirstApprovedAt == null || invoice.FirstApprovedAt == default(DateTime) ||
            invoice.FirstApprovedAt.Value.Kind != DateTimeKind.Utc || invoice.Amount <= 500000m)
            throw new InvalidOperationException("Invoice has invalid or incomplete first-approval metadata.");
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
