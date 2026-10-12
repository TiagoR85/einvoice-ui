using Bunit;
using Einvoice.Ui.Components;
using Einvoice.Ui.Services;

namespace Einvoice.Ui.Tests.Unit;

public sealed class InvoiceLinesEditorTests : BunitContext
{
    [Fact]
    public void Add_and_remove_rows_mutates_the_shared_list()
    {
        var lines = new List<CreateInvoiceLineRequest> { new("Initial", 1, 10, 0.23m) };
        var cut = Render<InvoiceLinesEditor>(p => p.Add(x => x.Lines, lines));

        Assert.Single(cut.FindAll("[data-testid=line-row]"));

        cut.Find("button.btn-outline-primary").Click();
        Assert.Equal(2, cut.FindAll("[data-testid=line-row]").Count);
        Assert.Equal(2, lines.Count);

        cut.FindAll("button.btn-outline-danger")[1].Click();
        Assert.Single(lines);
        Assert.Equal("Initial", lines[0].Description);
    }

    [Fact]
    public void Changing_description_updates_the_shared_list()
    {
        var lines = new List<CreateInvoiceLineRequest> { new("", 1, 0, 0.23m) };
        var cut = Render<InvoiceLinesEditor>(p => p.Add(x => x.Lines, lines));

        cut.Find("[data-testid=line-row] input").Change("Advisory");

        Assert.Equal("Advisory", lines[0].Description);
    }
}
