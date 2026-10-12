using Einvoice.Ui.Services;
using Minio;
using Minio.DataModel.Args;
using Testcontainers.Minio;

namespace Einvoice.Ui.Tests.Integration;

public sealed class SiloDocumentClientTests : IAsyncLifetime
{
    private const int MinioPort = 9000;
    private const string Bucket = "einvoice-docs";
    private MinioContainer _container = null!;
    private SiloDocumentClient _sut = null!;

    public async Task InitializeAsync()
    {
        _container = new MinioBuilder("pgsty/silo:RELEASE.2026-09-16T00-00-00Z")
            .WithUsername("minioadmin")
            .WithPassword("minioadmin")
            .Build();
        await _container.StartAsync();

        var endpoint = $"{_container.Hostname}:{_container.GetMappedPublicPort(MinioPort)}";
        _sut = new SiloDocumentClient(endpoint, _container.GetAccessKey(), _container.GetSecretKey(), Bucket);

        var uploader = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(_container.GetAccessKey(), _container.GetSecretKey())
            .WithSSL(false)
            .Build();
        await uploader.MakeBucketAsync(new MakeBucketArgs().WithBucket(Bucket));
        var pdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        await uploader.PutObjectAsync(new PutObjectArgs()
            .WithBucket(Bucket)
            .WithObject("invoices/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.pdf")
            .WithStreamData(new MemoryStream(pdf))
            .WithObjectSize(pdf.Length)
            .WithContentType("application/pdf"));
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact]
    public async Task Existing_pdf_returns_bytes()
    {
        var bytes = await _sut.GetPdfAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.NotNull(bytes);
        Assert.Equal(0x25, bytes![0]);
    }

    [Fact]
    public async Task Missing_pdf_returns_null()
    {
        var bytes = await _sut.GetPdfAsync(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        Assert.Null(bytes);
    }
}
