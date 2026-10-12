using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Einvoice.Ui.Services;

public sealed class SiloDocumentClient : IDocumentClient
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    public SiloDocumentClient(string endpoint, string accessKey, string secretKey, string bucket)
    {
        _bucket = bucket;
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task<byte[]?> GetPdfAsync(Guid invoiceId, CancellationToken ct = default)
    {
        try
        {
            var buffer = new MemoryStream();
            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject($"invoices/{invoiceId}.pdf")
                .WithCallbackStream(stream => stream.CopyTo(buffer)), ct);
            return buffer.ToArray();
        }
        catch (Exception ex) when (IsNotFound(ex))
        {
            return null;
        }
    }

    private static bool IsNotFound(Exception ex) =>
        ex is ObjectNotFoundException or BucketNotFoundException ||
        ex.GetType().Name.Contains("NotFound") ||
        ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("Not Found", StringComparison.OrdinalIgnoreCase);
}
