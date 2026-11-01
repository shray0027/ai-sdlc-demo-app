using System.Text.Json.Serialization;
using InvoiceApp.Api;
using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
        new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { error = "Invalid request input: " + string.Join("; ", context.ModelState.Values.SelectMany(value => value.Errors).Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Malformed request value." : error.ErrorMessage)) }))
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new ExactDecimalConverter());
    });

builder.Services.Configure<DemoUsersOptions>(builder.Configuration);
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
builder.Services.AddScoped<ApprovalService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<SessionAuthMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }
