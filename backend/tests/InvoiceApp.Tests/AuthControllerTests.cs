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

public class AuthControllerTests
{
    [Theory]
    [InlineData("demo.approver", "Manager")]
    [InlineData("demo.director1", "Director")]
    [InlineData("demo.director2", "Director")]
    [InlineData("demo.admin", "Admin")]
    public async Task Login_MigratesConfiguredAccounts(string username, string role)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password = "ChangeMe123!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(username, json.GetProperty("username").GetString()); Assert.Equal(role, json.GetProperty("role").GetString());
    }

}
