using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using static InvoiceApp.Tests.ApiTestSupport;

namespace InvoiceApp.Tests;

public class InvoicesControllerTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.001")]
    [InlineData("50000.001")]
    [InlineData("500000.001")]
    [InlineData("\"not a number\"")]
    [InlineData("792281625142643375935439503350")]
    [InlineData("79228162514264337593543950335.1")]
    [InlineData("50000.0000000000000000000000001")]
    [InlineData("500000.000000000000000000000001")]
    [InlineData("0.0099999999999999999999999999999")]
    public async Task InvalidCreation_Returns400AndAddsNothing(string amount)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Manager");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var before = repo.GetAll().Select(InvoiceState.Snapshot).ToArray();
        var response = await client.PostAsync("/api/invoices", new StringContent("{\"customerName\":\"test\",\"amount\":" + amount + "}", Encoding.UTF8, "application/json"));
        await Error(response, HttpStatusCode.BadRequest); Assert.Equal(before, repo.GetAll().Select(InvoiceState.Snapshot));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("50000.000")]
    [InlineData("5e4")]
    [InlineData("1e-2")]
    [InlineData("79228162514264337593543950335")]
    public async Task ValidNumericCents_CreationPreservesExactDecimal(string amount)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Manager");
        var response = await client.PostAsync("/api/invoices", new StringContent("{\"customerName\":\"test\",\"amount\":" + amount + "}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(decimal.Parse(amount, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture), json.GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task Admin_CanDeleteAndManagerCannot()
    {
        using var app = new WebApplicationFactory<Program>(); using var manager = Session(app, "Manager"); using var admin = Session(app, "Admin");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = repo.Add(InvoiceState.Pending());
        await Error(await manager.DeleteAsync($"/api/invoices/{invoice.Id}"), HttpStatusCode.Forbidden);
        Assert.Equal(InvoiceState.Snapshot(invoice), InvoiceState.Snapshot(repo.GetById(invoice.Id)));
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/invoices/{invoice.Id}")).StatusCode); Assert.Null(repo.GetById(invoice.Id));
    }

}
