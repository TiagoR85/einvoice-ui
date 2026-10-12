namespace Einvoice.Ui.Services;

public sealed class HubApiException(int statusCode, string? detail)
    : Exception(detail ?? $"Hub API returned {statusCode}.")
{
    public int StatusCode { get; } = statusCode;
    public string? Detail { get; } = detail;
}
