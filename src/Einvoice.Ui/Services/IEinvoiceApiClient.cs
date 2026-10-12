namespace Einvoice.Ui.Services;

public interface IEinvoiceApiClient
{
    Task<List<InvoiceResponse>> GetInvoicesAsync(CancellationToken ct = default);
    Task<InvoiceResponse?> GetInvoiceAsync(Guid id, CancellationToken ct = default);
    Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default);
    Task<List<PartyResponse>> GetPartiesAsync(CancellationToken ct = default);
    Task<PartyResponse> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct = default);
}
