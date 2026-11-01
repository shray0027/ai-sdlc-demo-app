using InvoiceApp.Domain;

namespace InvoiceApp.Api;

/// <summary>In-memory store so the demo app runs with no database. Seeded with a few invoices.</summary>
public class InMemoryInvoiceRepository : IInvoiceRepository
{
    private readonly object _lock = new();
    private int _nextNumber = 1006;

    private readonly List<Invoice> _invoices = new()
    {
        new Invoice { Id = "INV-1001", CustomerName = "Lakeside School District", Amount = 4250.00m, Status = InvoiceStatus.PendingApproval },
        new Invoice { Id = "INV-1002", CustomerName = "Hilltop Academy", Amount = 980.50m, Status = InvoiceStatus.PendingApproval },
        new Invoice { Id = "INV-1003", CustomerName = "Riverside Charter", Amount = 12300.00m, Status = InvoiceStatus.Approved, DecidedBy = "demo.approver", DecidedAt = DateTime.UtcNow.AddDays(-2) },
        new Invoice { Id = "INV-1004", CustomerName = "Oakwood Independent", Amount = 615.75m, Status = InvoiceStatus.PendingApproval },
        new Invoice { Id = "INV-1005", CustomerName = "Maple Grove Prep", Amount = 310.00m, Status = InvoiceStatus.Draft },
    };

    public IReadOnlyList<Invoice> GetAll()
    {
        lock (_lock) return _invoices.Select(i => i.Snapshot()).ToList();
    }

    public Invoice? GetById(string id)
    {
        lock (_lock) return _invoices.FirstOrDefault(i => i.Id == id)?.Snapshot();
    }

    public IReadOnlyList<Invoice> GetByIds(IEnumerable<string> ids)
    {
        var wanted = ids.ToHashSet();
        lock (_lock) return _invoices.Where(i => wanted.Contains(i.Id)).Select(i => i.Snapshot()).ToList();
    }

    public Invoice Add(Invoice invoice)
    {
        lock (_lock)
        {
            invoice = invoice.Snapshot();
            if (string.IsNullOrEmpty(invoice.Id))
            {
                invoice.Id = $"INV-{_nextNumber++}";
            }

            if (_invoices.Any(i => i.Id == invoice.Id))
                throw new InvalidOperationException($"Invoice {invoice.Id} already exists.");
            _invoices.Add(invoice);
            return invoice.Snapshot();
        }
    }

    public Invoice Mutate(string id, Action<Invoice> transition)
    {
        lock (_lock)
        {
            var index = _invoices.FindIndex(i => i.Id == id);
            if (index < 0) throw new KeyNotFoundException($"Invoice {id} was not found.");
            var working = _invoices[index].Snapshot();
            transition(working);
            if (working.Id != id) throw new InvalidOperationException("Invoice identity cannot change.");
            _invoices[index] = working.Snapshot();
            return working.Snapshot();
        }
    }

    public bool Remove(string id)
    {
        lock (_lock) return _invoices.RemoveAll(i => i.Id == id) > 0;
    }
}
