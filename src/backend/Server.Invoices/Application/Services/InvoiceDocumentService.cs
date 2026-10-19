using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Domain;

namespace Server.Invoices.Application.Services;

public sealed class InvoiceDocumentService : IInvoiceDocumentService
{
    public byte[] Generate(GeneratedInvoice invoice)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(style => style.FontSize(10).FontFamily(Fonts.Arial));
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(invoice.BusinessName).FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text(invoice.BusinessAddress).FontColor(Colors.Grey.Darken1);
                        column.Item().Text(invoice.BusinessEmail).FontColor(Colors.Grey.Darken1);
                        if (!string.IsNullOrWhiteSpace(invoice.BusinessPhone))
                            column.Item().Text(invoice.BusinessPhone).FontColor(Colors.Grey.Darken1);
                    });
                    row.ConstantItem(190).AlignRight().Column(column =>
                    {
                        column.Item().AlignRight().Text("INVOICE").FontSize(28).Bold();
                        column.Item().AlignRight().Text(invoice.InvoiceNumber).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingTop(8).AlignRight().Text($"Issued {invoice.IssueDate:dd MMM yyyy}");
                        column.Item().AlignRight().Text($"Due {invoice.DueDate:dd MMM yyyy}");
                    });
                });
                page.Content().PaddingVertical(28).Column(column =>
                {
                    column.Spacing(20);
                    column.Item().Background(Colors.Grey.Lighten4).Padding(14).Column(billTo =>
                    {
                        billTo.Item().Text("BILL TO").Bold().FontColor(Colors.Grey.Darken1);
                        billTo.Item().Text(invoice.ClientName).Bold();
                        billTo.Item().Text(invoice.ClientAddress);
                        billTo.Item().Text(invoice.ClientEmail);
                    });
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "Description");
                            HeaderCell(header.Cell(), "Project");
                            HeaderCell(header.Cell().AlignRight(), "Amount");
                        });
                        foreach (var record in invoice.WorkRecords.OrderBy(record => record.CreatedAt))
                        {
                            BodyCell(table.Cell(), record.Description);
                            BodyCell(table.Cell(), record.Project.Name);
                            BodyCell(table.Cell().AlignRight(), Money(record.Value, invoice.Currency));
                        }
                    });
                    column.Item().AlignRight().Width(220).BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("Total").Bold().FontSize(13);
                        row.RelativeItem().AlignRight().Text(Money(invoice.Subtotal, invoice.Currency)).Bold().FontSize(13);
                    });
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        column.Item().Column(notes =>
                        {
                            notes.Item().Text("Notes").Bold();
                            notes.Item().Text(invoice.Notes);
                        });
                    }
                });
                page.Footer().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text(Registration(invoice)).FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(100).AlignRight().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void HeaderCell(IContainer cell, string text) =>
        cell.Background(Colors.Blue.Darken3).Padding(8).Text(text).Bold().FontColor(Colors.White);

    private static void BodyCell(IContainer cell, string text) =>
        cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(9).PaddingHorizontal(8).Text(text);

    private static string Money(decimal amount, string currency) => $"{currency} {amount:N2}";

    private static string Registration(GeneratedInvoice invoice)
    {
        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(invoice.CompanyNumber))
            details.Add($"Company no. {invoice.CompanyNumber}");
        if (!string.IsNullOrWhiteSpace(invoice.VatNumber))
            details.Add($"VAT no. {invoice.VatNumber}");
        return string.Join("  |  ", details);
    }
}
