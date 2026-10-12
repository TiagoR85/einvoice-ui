using System.Net;
using Einvoice.Ui.Services;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Einvoice.Ui.Tests.Unit;

public sealed class OAuthTokenProviderTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Dispose();

    private OAuthTokenProvider Provider() => new(
        new HttpClient { BaseAddress = new Uri(_server.Url!) },
        new OAuthClientOptions("/connect/token", "einvoice-client", "secret", "invoice:read invoice:write"));

    [Fact]
    public async Task Fetches_and_caches_token()
    {
        _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBodyAsJson(new { access_token = "tok-1", expires_in = 3600, token_type = "Bearer" }));

        var provider = Provider();
        var first = await provider.GetTokenAsync();
        var second = await provider.GetTokenAsync();

        Assert.Equal("tok-1", first);
        Assert.Equal("tok-1", second);
        Assert.Single(_server.LogEntries); // second call served from cache
    }

    [Fact]
    public async Task Non_success_throws_hub_api_exception()
    {
        _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));

        await Assert.ThrowsAsync<HubApiException>(() => Provider().GetTokenAsync());
    }
}
