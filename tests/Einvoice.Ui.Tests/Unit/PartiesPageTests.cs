using Bunit;
using Einvoice.Ui.Components.Pages;
using Einvoice.Ui.Services;
using Einvoice.Ui.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Einvoice.Ui.Tests.Unit;

public sealed class PartiesPageTests : BunitContext
{
    private readonly FakeEinvoiceApiClient _api = new();

    private IRenderedComponent<Parties> RenderPage()
    {
        Services.AddSingleton<IEinvoiceApiClient>(_api);
        return Render<Parties>();
    }

    [Fact]
    public void Empty_state_renders_form()
    {
        var cut = RenderPage();
        cut.WaitForAssertion(() => Assert.Contains("New party", cut.Markup));
    }

    [Fact]
    public void Create_adds_party_to_list()
    {
        var cut = RenderPage();
        cut.WaitForAssertion(() => Assert.Contains("New party", cut.Markup));

        cut.FindAll("input[type=text]")[0].Change("Nova Lda");
        cut.FindAll("input[type=text]")[1].Change("PT");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Nova Lda", cut.Markup));
        Assert.Single(_api.Parties);
        Assert.Equal("Nova Lda", _api.Parties[0].Name);
    }
}
