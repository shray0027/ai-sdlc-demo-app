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
        foreach (var field in new[] { "firstApprovedBy", "firstApprovedRole", "firstApprovedAt" })
            Assert.Equal(JsonValueKind.Null, json.GetProperty(field).ValueKind);
    }

    public static IEnumerable<object[]> Denials()
    {
        yield return new object[] { "Manager", "actor", 50000.01m, InvoiceStatus.PendingApproval, 409, "50,000" };
        yield return new object[] { "Manager", "actor", 500000m, InvoiceStatus.PendingApproval, 409, "50,000" };
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
    private static async Task<string> Login(HttpClient client, string username)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password = "ChangeMe123!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    private static async Task AssertInvoiceResponse(HttpResponseMessage response, Invoice expected)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(json.Deserialize<Invoice>(options)));
        Assert.Equal(11, json.EnumerateObject().Count());
        foreach (var property in new[] { "firstApprovedBy", "firstApprovedRole", "firstApprovedAt" }) Assert.True(json.TryGetProperty(property, out _));
    }

    [Theory]
    [InlineData("demo.approver", "Manager")]
    [InlineData("demo.director1", "Director")]
    public async Task HighValue_RealLoginsFirstAndSecondAndFreshGetExposeFullHistory(string firstUsername, string firstRole)
    {
        using var app = new WebApplicationFactory<Program>(); using var firstClient = app.CreateClient(); using var secondClient = app.CreateClient();
        await Login(firstClient, firstUsername); await Login(secondClient, "demo.director2");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var expected = InvoiceState.HighValue(); repo.Add(expected);
        await AssertInvoiceResponse(await firstClient.GetAsync($"/api/invoices/{expected.Id}"), expected);
        var start = DateTime.UtcNow;
        var response = await firstClient.PostAsJsonAsync($"/api/approvals/{expected.Id}/approve", new { role = "Director", username = "forged" });
        var first = repo.GetById(expected.Id)!;
        Assert.InRange(first.FirstApprovedAt!.Value, start, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, first.FirstApprovedAt.Value.Kind);
        expected.FirstApprovedBy = firstUsername; expected.FirstApprovedRole = firstRole; expected.FirstApprovedAt = first.FirstApprovedAt;
        await AssertInvoiceResponse(response, expected);
        await AssertInvoiceResponse(await secondClient.GetAsync($"/api/invoices/{expected.Id}"), expected);
        var pending = InvoiceState.Snapshot(first);
        response = await secondClient.PostAsync($"/api/approvals/{expected.Id}/approve", null);
        var final = repo.GetById(expected.Id)!;
        Assert.InRange(final.DecidedAt!.Value, first.FirstApprovedAt.Value, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, final.DecidedAt.Value.Kind);
        expected.Status = InvoiceStatus.Approved; expected.DecidedBy = "demo.director2"; expected.DecidedAt = final.DecidedAt;
        await AssertInvoiceResponse(response, expected);
        await AssertInvoiceResponse(await firstClient.GetAsync($"/api/invoices/{expected.Id}"), expected);
        Assert.Equal(pending, InvoiceState.Snapshot(first));
    }

    [Fact]
    public async Task SameDirector_RepeatedAndActualReloginTokenCannotCountTwice()
    {
        using var app = new WebApplicationFactory<Program>(); using var client = app.CreateClient();
        var token = await Login(client, "demo.director1");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = repo.Add(InvoiceState.HighValue());
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/approvals/{invoice.Id}/approve", null)).StatusCode);
        var before = InvoiceState.Snapshot(repo.GetById(invoice.Id));
        await Error(await client.PostAsync($"/api/approvals/{invoice.Id}/approve", null), HttpStatusCode.Conflict, "distinct approver");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
        var newToken = await Login(client, "demo.director1"); Assert.NotEqual(token, newToken);
        await Error(await client.PostAsJsonAsync($"/api/approvals/{invoice.Id}/approve", new { username = "demo.director2", role = "Director" }), HttpStatusCode.Conflict, "distinct approver");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
        using var oldSession = app.CreateClient(); oldSession.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await Error(await oldSession.PostAsync($"/api/approvals/{invoice.Id}/approve", null), HttpStatusCode.Conflict, "distinct approver");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }

    [Theory]
    [InlineData("Manager", "same", "Director", "distinct approver")]
    [InlineData("Director", "same", "Manager", "distinct approver")]
    [InlineData("Director", "other", "Manager", "Director")]
    public async Task SecondApproval_UsesCanonicalSessionIdentityAndRole(string firstRole, string secondIdentity, string secondRole, string reason)
    {
        using var app = new WebApplicationFactory<Program>(); using var first = Session(app, firstRole, "same"); using var second = Session(app, secondRole, secondIdentity);
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var invoice = repo.Add(InvoiceState.HighValue());
        Assert.Equal(HttpStatusCode.OK, (await first.PostAsync($"/api/approvals/{invoice.Id}/approve", null)).StatusCode);
        var before = InvoiceState.Snapshot(repo.GetById(invoice.Id));
        await Error(await second.PostAsJsonAsync($"/api/approvals/{invoice.Id}/approve", new { username = "forged", role = "Director" }), HttpStatusCode.Conflict, reason);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(invoice.Id)));
    }

    [Theory]
    [MemberData(nameof(ApprovalServiceTests.InvalidFirstMetadata), MemberType = typeof(ApprovalServiceTests))]
    public async Task MalformedFirstMetadata_ApprovalAndRejectFailWithoutMutation(string? identity, string? role, DateTime? at)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Director");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = InvoiceState.HighValue();
        input.FirstApprovedBy = identity; input.FirstApprovedRole = role; input.FirstApprovedAt = at; repo.Add(input);
        var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        await Error(await client.PostAsync($"/api/approvals/{input.Id}/approve", null), HttpStatusCode.Conflict, "metadata");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        await Error(await client.PostAsJsonAsync($"/api/approvals/{input.Id}/reject", new { reason = "intentional" }), HttpStatusCode.Conflict, "metadata");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Theory]
    [InlineData("Manager", true)]
    [InlineData("Director", true)]
    [InlineData("Manager", false)]
    [InlineData("Director", false)]
    public async Task IntentionalRejectionAfterFirst_PreservesHistoryAndTerminalFailures(string role, bool sameActor)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, role, sameActor ? "first.actor" : "other");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var expected = InvoiceState.HighValue(); InvoiceState.First(expected); repo.Add(expected);
        var before = InvoiceState.Snapshot(repo.GetById(expected.Id));
        await Error(await client.PostAsJsonAsync($"/api/approvals/{expected.Id}/reject", new { reason = " " }), HttpStatusCode.BadRequest, "reason");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(expected.Id)));
        var start = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync($"/api/approvals/{expected.Id}/reject", new { reason = "Intentional rejection" });
        var stored = repo.GetById(expected.Id)!; Assert.InRange(stored.DecidedAt!.Value, start, DateTime.UtcNow); Assert.Equal(DateTimeKind.Utc, stored.DecidedAt.Value.Kind);
        expected.Status = InvoiceStatus.Rejected; expected.DecidedBy = sameActor ? "first.actor" : "other"; expected.DecidedAt = stored.DecidedAt; expected.DecisionNote = "Intentional rejection";
        await AssertInvoiceResponse(response, expected);
        Assert.Equal(InvoiceState.Snapshot(expected), InvoiceState.Snapshot(stored));
        before = InvoiceState.Snapshot(stored);
        await Error(await client.PostAsJsonAsync($"/api/approvals/{expected.Id}/reject", new { reason = "again" }), HttpStatusCode.Conflict, "pending");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(expected.Id)));
        await Error(await client.PostAsync($"/api/approvals/{expected.Id}/approve", null), HttpStatusCode.Conflict, "pending");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(expected.Id)));
    }
    [Theory]
    [InlineData(false, "decidedBy")]
    [InlineData(false, "decidedAt")]
    [InlineData(false, "decisionNote")]
    [InlineData(true, "decidedBy")]
    [InlineData(true, "decidedAt")]
    [InlineData(true, "decisionNote")]
    public async Task InconsistentPendingFinalMetadata_IsNotClearedByApproveOrReject(bool hasFirst, string field)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Director");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = InvoiceState.HighValue();
        if (hasFirst) InvoiceState.First(input);
        if (field == "decidedBy") input.DecidedBy = "sentinel";
        if (field == "decidedAt") input.DecidedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        if (field == "decisionNote") input.DecisionNote = "sentinel note";
        repo.Add(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        await Error(await client.PostAsync($"/api/approvals/{input.Id}/approve", null), HttpStatusCode.Conflict, "metadata");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        await Error(await client.PostAsJsonAsync($"/api/approvals/{input.Id}/reject", new { reason = "reason" }), HttpStatusCode.Conflict, "metadata");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Approved)]
    [InlineData(InvoiceStatus.Rejected)]
    public async Task TerminalOrDraftWithFirstHistory_AllDecisionFailuresPreserveState(InvoiceStatus status)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, "Director");
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = InvoiceState.Pending(500000.01m); InvoiceState.First(input); input.Status = status;
        repo.Add(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        await Error(await client.PostAsync($"/api/approvals/{input.Id}/approve", null), HttpStatusCode.Conflict, "pending");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
        await Error(await client.PostAsJsonAsync($"/api/approvals/{input.Id}/reject", new { reason = "reason" }), HttpStatusCode.Conflict, "pending");
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }
    [Theory]
    [InlineData("Admin", "actor", "500000.01", 403, "authorized")]
    [InlineData("Approver", "actor", "500000.01", 403, "authorized")]
    [InlineData("unknown", "actor", "500000.01", 403, "authorized")]
    [InlineData("Director", " ", "500000.01", 400, "identity")]
    [InlineData("Director", "actor", "500000.001", 400, "whole cents")]
    [InlineData("Director", "actor", "0", 400, "whole cents")]
    public async Task ActorOrAmountDenials_PreservePopulatedFirstHistory(string role, string actor, string amount, int code, string reason)
    {
        using var app = new WebApplicationFactory<Program>(); using var client = Session(app, role, actor);
        var repo = app.Services.GetRequiredService<IInvoiceRepository>(); var input = InvoiceState.HighValue(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        InvoiceState.First(input); repo.Add(input); var before = InvoiceState.Snapshot(repo.GetById(input.Id));
        await Error(await client.PostAsJsonAsync($"/api/approvals/{input.Id}/approve", new { username = "forged", role = "Director" }), (HttpStatusCode)code, reason);
        Assert.Equal(before, InvoiceState.Snapshot(repo.GetById(input.Id)));
    }
}
