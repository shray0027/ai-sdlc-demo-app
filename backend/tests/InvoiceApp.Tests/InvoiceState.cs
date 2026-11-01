using System.Text.Json;
using InvoiceApp.Domain;

namespace InvoiceApp.Tests;

internal static class InvoiceState
{
    // Serialize every public field, independently of the production copy implementation.
    public static string Snapshot(Invoice? invoice) => JsonSerializer.Serialize(invoice);
    public static Invoice Pending(decimal amount = 100m) => new()
    {
        Id = "stage-one", CustomerName = "Sentinel customer", Amount = amount,
        Status = InvoiceStatus.PendingApproval,
        CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
        DecidedBy = "historical sentinel", DecidedAt = new DateTime(2021, 2, 3, 4, 5, 6, DateTimeKind.Utc),
        DecisionNote = "sentinel note"
    };
}
