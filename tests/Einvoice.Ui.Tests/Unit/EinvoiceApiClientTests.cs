using System.Net;
using Einvoice.Ui.Services;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Einvoice.Ui.Tests.Unit;

public sealed class EinvoiceApiClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();
    private readonly HttpClient _http;
    private readonly EinvoiceApiClient _client;

    public EinvoiceApiClientTests()
    {
        _http = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var tokens = new OAuthTokenProvider(_http,
            new OAuthClientOptions("/connect/token", "einvoice-client", "secret", "invoice:read invoice:write"));
        _client = new EinvoiceApiClient(_http, tokens);
    }

    public void Dispose()
    {
        _server.Dispose();
        _http.Dispose();
    }

    private void StubToken() => _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
        .RespondWith(Response.Create().WithStatusCode(200)
            .WithBodyAsJson(new { access_token = "t", expires_in = 3600, token_type = "Bearer" }));

    [Fact]
    public async Task GetInvoices_deserializes_list_and_sends_bearer()
    {
        StubToken();
        _server.Given(Request.Create().WithPath("/v1/invoices").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    [{"id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","number":"INV-1","issueDate":"2026-10-11",
                      "dueDate":"2026-11-11","currency":"EUR","status":"Issued",
                      "sellerId":"11111111-1111-1111-1111-111111111111","buyerId":"22222222-2222-2222-2222-222222222222",
                      "lines":[],"totalAmount":10}]
                    """));

        var invoices = await _client.GetInvoicesAsync();

        var single = Assert.Single(invoices);
        Assert.Equal("INV-1", single.Number);
        var get = _server.LogEntries.First(e => e.RequestMessage!.Path == "/v1/invoices");
        Assert.Equal("Bearer", get.RequestMessage!.Headers["Authorization"][0].Split(' ')[0]);
    }

    [Fact]
    public async Task CreateInvoice_sends_idempotency_key_and_parses_201()
    {
        StubToken();
        _server.Given(Request.Create().WithPath("/v1/invoices").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {"id":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","number":"INV-9","issueDate":"2026-10-12",
                     "dueDate":"2026-11-12","currency":"EUR","status":"Issued",
                     "sellerId":"11111111-1111-1111-1111-111111111111","buyerId":"22222222-2222-2222-2222-222222222222",
                     "lines":[],"totalAmount":5}
                    """));

        var created = await _client.CreateInvoiceAsync(new CreateInvoiceRequest(
            "INV-9", "2026-10-12", "2026-11-12", "EUR",
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            [new CreateInvoiceLineRequest("Consulting", 1, 5, 0.23m)]));

        Assert.Equal("INV-9", created.Number);
        var post = _server.LogEntries.First(e =>
            e.RequestMessage!.Method == "POST" && e.RequestMessage.Path == "/v1/invoices");
        Assert.True(post.RequestMessage.Headers.ContainsKey("Idempotency-Key"));
    }

    [Fact]
    public async Task Problem_response_throws_hub_api_exception_with_detail()
    {
        StubToken();
        _server.Given(Request.Create().WithPath("/v1/invoices").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400)
                .WithBodyAsJson(new { title = "Validation failed", detail = "Number is required.", status = 400 }));

        var ex = await Assert.ThrowsAsync<HubApiException>(() => _client.CreateInvoiceAsync(
            new CreateInvoiceRequest("", "2026-10-12", "2026-11-12", "EUR",
                Guid.NewGuid(), Guid.NewGuid(),
                [new CreateInvoiceLineRequest("x", 1, 1, 0)])));

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Number is required.", ex.Detail);
    }

    [Fact]
    public async Task GetParties_deserializes_list()
    {
        StubToken();
        _server.Given(Request.Create().WithPath("/v1/parties").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    [{"id":"11111111-1111-1111-1111-111111111111","name":"Acme Lda","country":"PT",
                      "vatNumber":"PT123","type":"Supplier"}]
                    """));

        var parties = await _client.GetPartiesAsync();

        var party = Assert.Single(parties);
        Assert.Equal("Acme Lda", party.Name);
    }
}
