using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvoiceApp.Api;

/// <summary>Reject numeric input that System.Decimal would silently round.</summary>
public sealed class ExactDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("Amount must be a JSON number.");
        using var document = JsonDocument.ParseValue(ref reader);
        var element = document.RootElement;
        if (!element.TryGetDecimal(out var value) || Normalize(element.GetRawText()) != Normalize(value.ToString(CultureInfo.InvariantCulture)))
            throw new JsonException("Amount must be an exact decimal value; rounding is not supported.");
        return value;
    }

    // Compare coefficient/exponent pairs instead of multiplying decimals. BigInteger
    // handles arbitrary JSON exponents without overflowing during validation.
    private static (string Coefficient, BigInteger Exponent) Normalize(string number)
    {
        var exponentPosition = number.IndexOfAny(new[] { 'e', 'E' });
        var exponent = exponentPosition < 0 ? BigInteger.Zero : BigInteger.Parse(number[(exponentPosition + 1)..], CultureInfo.InvariantCulture);
        var significand = exponentPosition < 0 ? number : number[..exponentPosition];
        var point = significand.IndexOf('.');
        if (point >= 0) { exponent -= significand.Length - point - 1; significand = significand.Remove(point, 1); }
        var negative = significand.StartsWith('-');
        var digits = significand.TrimStart('-').TrimStart('0');
        if (digits.Length == 0) return ("0", BigInteger.Zero);
        var trimmed = digits.TrimEnd('0');
        exponent += digits.Length - trimmed.Length;
        return ((negative ? "-" : "") + trimmed, exponent);
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}
