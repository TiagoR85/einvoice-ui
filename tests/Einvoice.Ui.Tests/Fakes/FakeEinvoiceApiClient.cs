using Einvoice.Ui.Services;

namespace Einvoice.Ui.Tests.Fakes;

public sealed class FakeEinvoiceApiClient : IEinvoiceApiClient
{
    public List<InvoiceResponse> Invoices { get; set; } = new();
    public List<PartyResponse> Parties { get; set; } = new();
    public CreateInvoiceRequest? LastCreatedInvoice { get; private set; }
    public Func<CreateInvoiceRequest, InvoiceResponse>? OnCreate { get; set; }
    public HubApiException? GetInvoicesError { get; set; }

    public Task<List<InvoiceResponse>> GetInvoicesAsync(CancellationToken ct = default)
    {
        if (GetInvoicesError is not null)
            throw GetInvoicesError;
        return Task.FromResult(Invoices);
    }

    public Task<InvoiceResponse?> GetInvoiceAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Invoices.FirstOrDefault(i => i.Id == id));

    public Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        LastCreatedInvoice = request;
        return Task.FromResult(OnCreate?.Invoke(request) ?? new InvoiceResponse(
            Guid.NewGuid(), request.Number, request.IssueDate, request.DueDate, request.Currency,
            "Issued", request.SellerId, request.BuyerId, [], 0m));
    }

    public Task<List<PartyResponse>> GetPartiesAsync(CancellationToken ct = default) =>
        Task.FromResult(Parties);

    public Task<PartyResponse> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct = default)
    {
        var party = new PartyResponse(Guid.NewGuid(), request.Name, request.Country, request.VatNumber, request.Type);
        Parties.Add(party);
        return Task.FromResult(party);
    }
}
