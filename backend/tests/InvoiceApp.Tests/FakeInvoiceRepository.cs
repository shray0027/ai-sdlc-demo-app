using InvoiceApp.Domain;

namespace InvoiceApp.Tests;

/// <summary>In-memory store so the demo app runs with no database. Seeded with a few invoices.</summary>
internal class FakeInvoiceRepository : IInvoiceRepository
{
    private readonly object _lock = new();
    private int _nextNumber = 1006;

    private readonly List<Invoice> _invoices;
    public FakeInvoiceRepository(params Invoice[] seed) => _invoices = seed.Select(i => i.Snapshot()).ToList();

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
