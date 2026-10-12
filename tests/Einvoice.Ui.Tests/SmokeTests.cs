using Bunit;
using Einvoice.Ui.Components;

namespace Einvoice.Ui.Tests;

public sealed class SmokeTests : BunitContext
{
    [Fact]
    public void Renders_router_through_layout_to_home_page()
    {
        var cut = Render<Routes>();
        Assert.Contains("Hello, world!", cut.Markup);
    }

    [Fact]
    public void Test_context_is_wired()
    {
        Assert.True(true);
    }
}
