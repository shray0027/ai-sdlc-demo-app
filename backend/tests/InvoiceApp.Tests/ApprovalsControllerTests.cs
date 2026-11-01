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

public class ApprovalsControllerTests
{
    [Theory]
    [InlineData("demo.approver", 50000)]
    [InlineData("demo.director1", 500000)]
    public async Task AuthenticatedRole_ReachesDomainInclusiveLimit(string username, decimal amount)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = app.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username, password = "ChangeMe123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var session = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.GetProperty("token").GetString());
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var expected = InvoiceState.Pending(amount);
        expected.DecidedBy = null; expected.DecidedAt = null; expected.DecisionNote = null; repo.Add(expected);
        var startedAt = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync($"/api/approvals/{expected.Id}/approve", new { username = "forged.actor", role = "Admin" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Approved", json.GetProperty("status").GetString()); Assert.Equal(username, json.GetProperty("decidedBy").GetString());
        var decidedAt = json.GetProperty("decidedAt").GetDateTime();
        Assert.InRange(decidedAt, startedAt, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, decidedAt.Kind);
        expected.Status = InvoiceStatus.Approved; expected.DecidedBy = username; expected.DecidedAt = decidedAt;
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(repo.GetById(expected.Id)));
        Assert.Equal(expected.Id, json.GetProperty("id").GetString()); Assert.Equal(expected.CustomerName, json.GetProperty("customerName").GetString());
        Assert.Equal(expected.Amount, json.GetProperty("amount").GetDecimal()); Assert.Equal(expected.CreatedAt, json.GetProperty("createdAt").GetDateTime());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("decisionNote").ValueKind);
    }

    public static IEnumerable<object[]> Denials()
    {
        yield return new object[] { "Manager", "actor", 50000.01m, InvoiceStatus.PendingApproval, 409, "50,000" };
        yield return new object[] { "Manager", "actor", 500000m, InvoiceStatus.PendingApproval, 409, "50,000" };
        foreach (var role in new[] { "Manager", "Director" })
            yield return new object[] { role, "actor", 500000.01m, InvoiceStatus.PendingApproval, 409, "Two-person" };
        foreach (var role in new[] { "Admin", "Approver", "unknown" })
            yield return new object[] { role, "actor", 100m, InvoiceStatus.PendingApproval, 403, "authorized" };
        foreach (var amount in new[] { 0m, -1m, 50000.001m, 500000.001m })
            yield return new object[] { "Director", "actor", amount, InvoiceStatus.PendingApproval, 400, "whole cents" };
        yield return new object[] { "Director", " ", 100m, InvoiceStatus.PendingApproval, 400, "identity" };
        foreach (var status in new[] { InvoiceStatus.Draft, InvoiceStatus.Approved, InvoiceStatus.Rejected })
            yield return new object[] { "Director", "actor", 100m, status, 409, "pending" };
    }

    [Theory]
    [MemberData(nameof(Denials))]
    public async Task RejectedApprovals_ReturnReasonAndPreserveAllPersistedFields(string role, string actor, decimal amount, InvoiceStatus status, int code, string reason)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, role, actor);
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = InvoiceState.Pending(amount); input.Status = status; repo.Add(input);
        var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        var response = await client.PostAsJsonAsync($"/api/approvals/{input.Id}/approve", new { username = "forged.director", role = "Director", approver = "forged.director" });
        await Error(response, (HttpStatusCode)code, reason);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrInvalidSession_Returns401WithoutChanges(bool invalidToken)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = app.CreateClient();
        if (invalidToken) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = repo.Add(InvoiceState.Pending()); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        await Error(await client.PostAsync($"/api/approvals/{input.Id}/approve", null), HttpStatusCode.Unauthorized);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Fact]
    public async Task MissingInvoice_Returns404AndRemainsAbsent()
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Director");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var before = repo.GetAll().Select(InvoiceState.Snapshot).ToArray();
        await Error(await client.PostAsync("/api/approvals/missing/approve", null), HttpStatusCode.NotFound, "not found");
        Assert.Null(repo.GetById("missing")); Assert.Equal(before, repo.GetAll().Select(InvoiceState.Snapshot));
    }

    [Fact]
    public async Task ForgedBodyAndHeaders_CannotReplaceAuthenticatedIdentity()
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Manager", "trusted.manager");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = InvoiceState.Pending(50000m); invoice.DecisionNote = null; repo.Add(invoice);
        client.DefaultRequestHeaders.Add("X-Role", "Director"); client.DefaultRequestHeaders.Add("X-Username", "forged.director");
        var response = await client.PostAsJsonAsync($"/api/approvals/{invoice.Id}/approve", new { username = "forged.director", role = "Director", approver = "forged.director" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Equal("trusted.manager", repo.GetById(invoice.Id)!.DecidedBy);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Approver")]
    [InlineData("unknown")]
    public async Task UnsupportedRoles_CannotReject(string role)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, role);
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = repo.Add(InvoiceState.Pending()); var before = InvoiceState.Snapshot(invoice);
        await Error(await client.PostAsJsonAsync($"/api/approvals/{invoice.Id}/reject", new { reason = "duplicate" }), HttpStatusCode.Forbidden);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\"reason\":123}")]
    [InlineData("{}")]
    public async Task MalformedRejection_ReturnsClear400(string body)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Manager");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = repo.Add(InvoiceState.Pending()); var before = InvoiceState.Snapshot(invoice);
        await Error(await client.PostAsync($"/api/approvals/{invoice.Id}/reject", new StringContent(body, Encoding.UTF8, "application/json")), HttpStatusCode.BadRequest);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }
}
