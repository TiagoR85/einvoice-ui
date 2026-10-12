using Bunit;
using Einvoice.Ui.Components.Pages;
using Einvoice.Ui.Services;
using Einvoice.Ui.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Einvoice.Ui.Tests.Unit;

public sealed class InvoicesPageTests : BunitContext
{
    private static InvoiceResponse Invoice(string number, string status) => new(
        Guid.NewGuid(), number, "2026-10-11", "2026-11-11", "EUR", status,
        Guid.NewGuid(), Guid.NewGuid(),
        [new InvoiceLineResponse(Guid.NewGuid(), "Consulting", 1, 100, 0.23m, 100)], 100m);

    private IRenderedComponent<Invoices> RenderPage(FakeEinvoiceApiClient api)
    {
        Services.AddSingleton<IEinvoiceApiClient>(api);
        return Render<Invoices>();
    }

    [Fact]
    public void Shows_invoice_rows_with_status_badges()
    {
        var api = new FakeEinvoiceApiClient
        {
            Invoices = [Invoice("INV-1", "Issued"), Invoice("INV-2", "Completed")]
        };
        var cut = RenderPage(api);

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("INV-1", cut.Markup);
            Assert.Contains("text-bg-secondary", cut.Markup);
            Assert.Contains("text-bg-success", cut.Markup);
        });
    }

    [Fact]
    public void Completed_row_offers_pdf_link()
    {
        var api = new FakeEinvoiceApiClient { Invoices = [Invoice("INV-2", "Completed")] };
        var cut = RenderPage(api);

        cut.WaitForAssertion(() => Assert.Contains("/pdf/", cut.Markup));
    }

    [Fact]
    public void Issued_row_has_no_pdf_link()
    {
        var api = new FakeEinvoiceApiClient { Invoices = [Invoice("INV-1", "Issued")] };
        var cut = RenderPage(api);

        cut.WaitForAssertion(() => Assert.Contains("INV-1", cut.Markup));
        Assert.DoesNotContain("/pdf/", cut.Markup);
    }

    [Fact]
    public void Empty_state_renders_hint()
    {
        var cut = RenderPage(new FakeEinvoiceApiClient());

        cut.WaitForAssertion(() => Assert.Contains("No invoices yet", cut.Markup));
    }

    [Fact]
    public void Api_failure_shows_warning_banner()
    {
        var cut = RenderPage(new FakeEinvoiceApiClient
        {
            GetInvoicesError = new HubApiException(503, "Hub is down.")
        });

        cut.WaitForAssertion(() => Assert.Contains("Hub is down.", cut.Markup));
    }
}
