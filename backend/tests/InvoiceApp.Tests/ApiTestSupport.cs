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

namespace InvoiceApp.Tests;

internal static class ApiTestSupport
{
    internal static HttpClient Session(WebApplicationFactory<Program> app, string role, string identity = "canonical.actor")
    {
        var token = app.Services.GetRequiredService<SessionStore>().Create(new UserSession(identity, role));
        var client = app.CreateClient(); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); return client;
    }
    internal static async Task Error(HttpResponseMessage response, HttpStatusCode status, string? reason = null)
    {
        Assert.Equal(status, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var message = json.GetProperty("error").GetString(); Assert.False(string.IsNullOrWhiteSpace(message));
        if (reason != null) Assert.Contains(reason, message);
    }

}
