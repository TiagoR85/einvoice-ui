using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Einvoice.Ui.Services;

public sealed class EinvoiceApiClient : IEinvoiceApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly OAuthTokenProvider _tokens;

    public EinvoiceApiClient(HttpClient http, OAuthTokenProvider tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    public Task<List<InvoiceResponse>> GetInvoicesAsync(CancellationToken ct = default) =>
        GetAsync<List<InvoiceResponse>>("/v1/invoices", ct);

    public async Task<InvoiceResponse?> GetInvoiceAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<InvoiceResponse>($"/v1/invoices/{id}", ct);
        }
        catch (HubApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public Task<List<PartyResponse>> GetPartiesAsync(CancellationToken ct = default) =>
        GetAsync<List<PartyResponse>>("/v1/parties", ct);

    public async Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        using var message = await SendAsync(HttpMethod.Post, "/v1/invoices", request, ct);
        message.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await ReadAsync<InvoiceResponse>(message, ct);
    }

    public async Task<PartyResponse> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct = default)
    {
        using var message = await SendAsync(HttpMethod.Post, "/v1/parties", request, ct);
        return await ReadAsync<PartyResponse>(message, ct);
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var message = await SendAsync(HttpMethod.Get, path, null, ct);
        return await ReadAsync<T>(message, ct);
    }

    private async Task<HttpRequestMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var token = await _tokens.GetTokenAsync(ct);
        var message = new HttpRequestMessage(method, path);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            message.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        return message;
    }

    private async Task<T> ReadAsync<T>(HttpRequestMessage message, CancellationToken ct)
    {
        using var response = await _http.SendAsync(message, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HubApiException((int)response.StatusCode, ExtractDetail(raw, (int)response.StatusCode));
        return JsonSerializer.Deserialize<T>(raw, Json)
            ?? throw new HubApiException(502, "Empty response body.");
    }

    private static string? ExtractDetail(string raw, int statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString();
        }
        catch (JsonException)
        {
            // not a ProblemDetails body
        }
        return $"Hub API returned {statusCode}.";
    }
}
