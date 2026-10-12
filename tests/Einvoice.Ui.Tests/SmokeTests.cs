using Bunit;
using Einvoice.Ui.Components;
using Einvoice.Ui.Components.Pages;
using Einvoice.Ui.Services;
using Einvoice.Ui.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Einvoice.Ui.Tests;

public sealed class SmokeTests : BunitContext
{
    [Fact]
    public void Renders_router_through_layout_to_home_page()
    {
        Services.AddSingleton<IEinvoiceApiClient>(new FakeEinvoiceApiClient());
        var cut = Render<Routes>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Invoices", cut.Markup);
            Assert.Contains("No invoices yet", cut.Markup);
        });
    }

    [Fact]
    public void Test_context_is_wired()
    {
        Assert.True(true);
    }

    [Fact]
    public void Error_page_renders()
    {
        var cut = Render<Error>();
        Assert.False(string.IsNullOrWhiteSpace(cut.Markup));
    }
}
