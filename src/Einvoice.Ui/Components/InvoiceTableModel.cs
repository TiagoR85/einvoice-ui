using Einvoice.Ui.Services;

namespace Einvoice.Ui.Components;

public sealed record InvoiceTableModel(
    Guid Id, string Number, string IssueDate, decimal TotalAmount, string Currency, string Status)
{
    public static InvoiceTableModel From(InvoiceResponse invoice) =>
        new(invoice.Id, invoice.Number, invoice.IssueDate, invoice.TotalAmount, invoice.Currency, invoice.Status);

    public bool IsTerminal => Status is "Completed" or "Failed";
}
