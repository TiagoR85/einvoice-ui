using Bunit;
using Einvoice.Ui.Components.Pages;
using Einvoice.Ui.Services;
using Einvoice.Ui.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Einvoice.Ui.Tests.Unit;

public sealed class InvoiceCreatePageTests : BunitContext
{
    private readonly FakeEinvoiceApiClient _api = new()
    {
        Parties =
        [
            new PartyResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Acme Lda", "PT", "PT123", "Supplier"),
            new PartyResponse(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Globex SA", "ES", null, "Customer")
        ]
    };

    private IRenderedComponent<InvoiceCreate> RenderPage()
    {
        Services.AddSingleton<IEinvoiceApiClient>(_api);
        return Render<InvoiceCreate>();
    }

    [Fact]
    public void Loads_parties_into_selects()
    {
        var cut = RenderPage();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Acme Lda", cut.Markup);
            Assert.Contains("Globex SA", cut.Markup);
        });
    }

    [Fact]
    public void Submit_without_lines_shows_validation_error()
    {
        var cut = RenderPage();
        cut.WaitForAssertion(() => Assert.Contains("Acme Lda", cut.Markup));

        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.Contains("At least one line", cut.Markup));

        Assert.Null(_api.LastCreatedInvoice);
    }

    [Fact]
    public void Valid_form_creates_invoice()
    {
        var cut = RenderPage();
        cut.WaitForAssertion(() => Assert.Contains("Acme Lda", cut.Markup));

        cut.FindAll("input[type=text]")[0].Change("INV-100");
        cut.FindAll("input[type=date]")[0].Change("2026-10-12");
        cut.FindAll("input[type=date]")[1].Change("2026-11-12");
        cut.FindAll("input[type=text]")[1].Change("EUR");
        cut.Find("[data-testid=line-row] input").Change("Consulting");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(_api.LastCreatedInvoice));
        Assert.Equal("INV-100", _api.LastCreatedInvoice!.Number);
        Assert.Equal("Consulting", _api.LastCreatedInvoice!.Lines[0].Description);
    }
}
