using Bunit;
using Einvoice.Ui.Components;

namespace Einvoice.Ui.Tests.Unit;

public sealed class StatusBadgeTests : BunitContext
{
    [Theory]
    [InlineData("Issued", "badge", "text-bg-secondary")]
    [InlineData("Processing", "badge", "text-bg-info")]
    [InlineData("Completed", "badge", "text-bg-success")]
    [InlineData("Failed", "badge", "text-bg-danger")]
    public void Renders_badge_class_per_status(string status, string c1, string c2)
    {
        var cut = Render<StatusBadge>(p => p.Add(x => x.Status, status));
        Assert.Contains(c1, cut.Markup);
        Assert.Contains(c2, cut.Markup);
        Assert.Contains(status, cut.Markup);
    }

    [Fact]
    public void Unknown_status_falls_back_to_secondary()
    {
        var cut = Render<StatusBadge>(p => p.Add(x => x.Status, "Something"));
        Assert.Contains("text-bg-secondary", cut.Markup);
    }
}
