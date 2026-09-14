using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services.Pdf;

public interface ITicketPdfGenerator
{
    byte[] GenerateTicketSummary(Ticket ticket);
}

public class TicketPdfGenerator : ITicketPdfGenerator
{
    public byte[] GenerateTicketSummary(Ticket ticket)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, ticket));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Support-Pulse").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Official Ticket Summary Report").FontSize(14).FontColor(Colors.Grey.Medium);
            });
            row.ConstantItem(100).AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(10);
        });
    }

    private void ComposeContent(IContainer container, Ticket ticket)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(15);

            column.Item().Text($"Ticket #{ticket.Id} - {ticket.Title}").FontSize(18).SemiBold();
            column.Item().Text($"Status: {ticket.Status} | Category: {ticket.Category}").FontColor(Colors.Grey.Darken2);
            
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Text("Customer Details").FontSize(14).SemiBold();
            column.Item().Text($"Name: {ticket.Customer?.Name ?? "Unknown"}");
            column.Item().Text($"Email: {ticket.Customer?.Email ?? "N/A"}");

            column.Item().PaddingTop(10).Text("Ticket Description").FontSize(14).SemiBold();
            column.Item().Text(ticket.Description);

            // Agar AI Summary available hai, toh ek highlighted box me dikhayenge
            if (!string.IsNullOrEmpty(ticket.AiSummary))
            {
                column.Item().PaddingTop(15).Background(Colors.Blue.Lighten5).Padding(15).Column(aiBox =>
                {
                    aiBox.Item().Text("🤖 AI Analysis & Insights").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                    aiBox.Spacing(5);
                    aiBox.Item().Text($"Sentiment: {ticket.AiSentiment}").Bold();
                    aiBox.Item().Text($"Predicted Category: {ticket.AiCategory}").Bold();
                    aiBox.Item().PaddingTop(5).Text(ticket.AiSummary);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}