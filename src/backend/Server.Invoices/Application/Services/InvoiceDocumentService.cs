using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Domain;

namespace Server.Invoices.Application.Services;

public sealed class InvoiceDocumentService : IInvoiceDocumentService
{
    private static readonly DateTimeOffset DemonstrationDate =
        new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    public byte[] GenerateDemonstration(string style, byte[]? logoData)
    {
        var project = new Project { Name = "Website redesign" };
        var invoice = new GeneratedInvoice
        {
            InvoiceNumber = "INV-DEMO-001",
            InvoiceStyle = style,
            LogoData = logoData,
            BusinessName = "Your Business",
            BusinessEmail = "hello@yourbusiness.example",
            BusinessPhone = "+44 20 7946 0123",
            BusinessAddress = "10 Example Street, London, SW1A 1AA",
            CompanyNumber = "12345678",
            VatNumber = "GB123456789",
            ClientName = "Example Client Ltd",
            ClientEmail = "accounts@example-client.test",
            ClientAddress = "25 Sample Road, Bristol, BS1 1AA",
            IssueDate = new DateOnly(2026, 10, 24),
            DueDate = new DateOnly(2026, 11, 23),
            Currency = "GBP",
            Subtotal = 1_500m,
            Notes = "Thank you for your business.",
            WorkRecords =
            [
                new Invoice
                {
                    Description = "Design and implementation",
                    Value = 1_200m,
                    Project = project,
                    CreatedAt = DemonstrationDate
                },
                new Invoice
                {
                    Description = "Project support",
                    Value = 300m,
                    Project = project,
                    CreatedAt = DemonstrationDate.AddMinutes(1)
                }
            ]
        };
        return Generate(invoice);
    }

    public byte[] Generate(GeneratedInvoice invoice)
    {
        var style = InvoiceStyle.For(invoice.InvoiceStyle);
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
                        if (invoice.LogoData is { Length: > 0 })
                            column.Item().Height(42).PaddingBottom(8).Image(invoice.LogoData).FitArea();
                        column.Item().Text(invoice.BusinessName).FontSize(style.BusinessNameSize).Bold().FontColor(style.Accent);
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
                    column.Item().Background(style.BillToBackground).Padding(14).Column(billTo =>
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
                            HeaderCell(header.Cell(), "Description", style);
                            HeaderCell(header.Cell(), "Project", style);
                            HeaderCell(header.Cell().AlignRight(), "Amount", style);
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

    private static void HeaderCell(IContainer cell, string text, InvoiceStyle style)
    {
        var header = cell.Background(style.HeaderBackground).Padding(8);
        if (style.HeaderBorder)
            header = header.BorderBottom(1).BorderColor(Colors.Grey.Darken2);
        header.Text(text).Bold().FontColor(style.HeaderText);
    }

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

    private sealed record InvoiceStyle(
        string Accent,
        string HeaderBackground,
        string HeaderText,
        string BillToBackground,
        float BusinessNameSize,
        bool HeaderBorder)
    {
        public static InvoiceStyle For(string style) => style switch
        {
            InvoiceStyles.Modern => new(
                Colors.Teal.Darken3,
                Colors.Teal.Darken3,
                Colors.White,
                Colors.Teal.Lighten5,
                24,
                false),
            InvoiceStyles.Minimal => new(
                Colors.Grey.Darken4,
                Colors.White,
                Colors.Grey.Darken4,
                Colors.White,
                18,
                true),
            _ => new(
                Colors.Blue.Darken3,
                Colors.Blue.Darken3,
                Colors.White,
                Colors.Grey.Lighten4,
                20,
                false)
        };
    }
}
