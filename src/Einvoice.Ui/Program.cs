using Einvoice.Ui.Components;
using Einvoice.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

string[] required = ["Api:BaseUrl", "Api:ClientSecret", "Minio:Endpoint"];
string[] missing = required
    .Where(key => string.IsNullOrWhiteSpace(builder.Configuration[key]))
    .ToArray();
if (missing.Length > 0)
    throw new InvalidOperationException(
        $"Missing required configuration: {string.Join(", ", missing)}. See .env.example.");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]!;
builder.Services.AddSingleton(new OAuthClientOptions(
    new Uri(new Uri(apiBaseUrl), "/connect/token").AbsoluteUri,
    builder.Configuration["Api:ClientId"] ?? "einvoice-client",
    builder.Configuration["Api:ClientSecret"]!,
    builder.Configuration["Api:Scope"] ?? "invoice:read invoice:write"));
builder.Services.AddHttpClient<OAuthTokenProvider>();
builder.Services.AddHttpClient<IEinvoiceApiClient, EinvoiceApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddSingleton<IDocumentClient>(_ => new SiloDocumentClient(
    builder.Configuration["Minio:Endpoint"]!,
    builder.Configuration["Minio:AccessKey"] ?? "minioadmin",
    builder.Configuration["Minio:SecretKey"] ?? "minioadmin",
    builder.Configuration["Minio:Bucket"] ?? "einvoice-docs"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/pdf/{id:guid}", async (Guid id, IDocumentClient documents, CancellationToken ct) =>
{
    var bytes = await documents.GetPdfAsync(id, ct);
    return bytes is null ? Results.NotFound() : Results.File(bytes, "application/pdf");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
