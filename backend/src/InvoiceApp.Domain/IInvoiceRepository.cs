namespace InvoiceApp.Domain;

public interface IInvoiceRepository
{
    IReadOnlyList<Invoice> GetAll();
    Invoice? GetById(string id);
    IReadOnlyList<Invoice> GetByIds(IEnumerable<string> ids);
    Invoice Add(Invoice invoice);
    /// <summary>Run a synchronous transition on a detached copy under the shared write lock.
    /// Exceptions discard all changes; callbacks must not reenter the repository.
    /// All reads, Add and Mutate return detached snapshots. No stored references escape.</summary>
    Invoice Mutate(string id, Action<Invoice> transition);
    bool Remove(string id);
}
