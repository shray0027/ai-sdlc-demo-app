namespace InvoiceApp.Domain;

public static class UsdAmount
{
    public static void Validate(decimal amount)
    {
        if (amount <= 0 || amount % 0.01m != 0)
            throw new ArgumentException("Amount must be positive USD in whole cents; fractional cents are not supported.", nameof(amount));
    }
}
