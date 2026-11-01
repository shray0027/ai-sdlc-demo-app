using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class UsdAmountTests
{
    [Theory]
    [InlineData("0.01")]
    [InlineData("50000.000")]
    [InlineData("79228162514264337593543950335")]
    [InlineData("792281625142643375935439503.35")]
    public void NumericCents_AcceptsWithoutMultiplicationOverflow(string amount) =>
        UsdAmount.Validate(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));

}
