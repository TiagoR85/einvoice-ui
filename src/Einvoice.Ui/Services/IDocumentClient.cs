namespace Einvoice.Ui.Services;

public interface IDocumentClient
{
    Task<byte[]?> GetPdfAsync(Guid invoiceId, CancellationToken ct = default);
}
