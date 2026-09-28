// using Backend.Models;
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;

// namespace Backend.Services.Pdf;

// public interface ITicketPdfGenerator
// {
//     byte[] GenerateTicketSummary(Ticket ticket);
// }

// public class TicketPdfGenerator : ITicketPdfGenerator
// {
//     public byte[] GenerateTicketSummary(Ticket ticket)
//     {
//         var document = Document.Create(container =>
//         {
//             container.Page(page =>
//             {
//                 page.Size(PageSizes.A4);
//                 page.Margin(2, Unit.Centimetre);
//                 page.PageColor(Colors.White);
//                 page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

//                 page.Header().Element(ComposeHeader);
//                 page.Content().Element(x => ComposeContent(x, ticket));
//                 page.Footer().Element(ComposeFooter);
//             });
//         });

//         return document.GeneratePdf();
//     }

//     private void ComposeHeader(IContainer container)
//     {
//         container.Row(row =>
//         {
//             row.RelativeItem().Column(column =>
//             {
//                 column.Item().Text("Support-Pulse").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
//                 column.Item().Text("Official Ticket Summary Report").FontSize(14).FontColor(Colors.Grey.Medium);
//             });
//             row.ConstantItem(100).AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(10);
//         });
//     }

//     private void ComposeContent(IContainer container, Ticket ticket)
//     {
//         container.PaddingVertical(1, Unit.Centimetre).Column(column =>
//         {
//             column.Spacing(15);

//             column.Item().Text($"Ticket #{ticket.Id} - {ticket.Title}").FontSize(18).SemiBold();
//             column.Item().Text($"Status: {ticket.Status} | Category: {ticket.Category}").FontColor(Colors.Grey.Darken2);
            
//             column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

//             column.Item().Text("Customer Details").FontSize(14).SemiBold();
//             column.Item().Text($"Name: {ticket.Customer?.Name ?? "Unknown"}");
//             column.Item().Text($"Email: {ticket.Customer?.Email ?? "N/A"}");

//             column.Item().PaddingTop(10).Text("Ticket Description").FontSize(14).SemiBold();
//             column.Item().Text(ticket.Description);

//             // Agar AI Summary available hai, toh ek highlighted box me dikhayenge
//             if (!string.IsNullOrEmpty(ticket.AiSummary))
//             {
//                 column.Item().PaddingTop(15).Background(Colors.Blue.Lighten5).Padding(15).Column(aiBox =>
//                 {
//                     aiBox.Item().Text("🤖 AI Analysis & Insights").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
//                     aiBox.Spacing(5);
//                     aiBox.Item().Text($"Sentiment: {ticket.AiSentiment}").Bold();
//                     aiBox.Item().Text($"Predicted Category: {ticket.AiCategory}").Bold();
//                     aiBox.Item().PaddingTop(5).Text(ticket.AiSummary);
//                 });
//             }
//         });
//     }

//     private void ComposeFooter(IContainer container)
//     {
//         container.AlignCenter().Text(x =>
//         {
//             x.Span("Page ");
//             x.CurrentPageNumber();
//             x.Span(" of ");
//             x.TotalPages();
//         });
//     }
// }

using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq; // Agent notes ko sort karne ke liye zaroori hai

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
                column.Item().Text("Support-Pulse").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken3);
                column.Item().Text("Official Ticket Summary Report").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
            });
            row.ConstantItem(150).AlignRight().Text($"Date Generated: {DateTime.Now:dd MMM yyyy}").FontSize(10).SemiBold();
        });
    }

    private void ComposeContent(IContainer container, Ticket ticket)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(10);

            // Top Metadata
            column.Item().Text($"Ticket ID: #{ticket.Id}").FontSize(14).Bold();
            column.Item().Text($"Status: {ticket.Status} | Customer: {ticket.Customer?.Name ?? "Unknown"} ({ticket.Customer?.Email ?? "N/A"})").FontColor(Colors.Grey.Darken3);
            
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // --- 1. CUSTOMER ISSUE ---
            column.Item().PaddingBottom(2).Text("--- 1. CUSTOMER ISSUE ---").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().Text(text =>
            {
                text.Span("Title: ").Bold();
                text.Span(ticket.Title);
            });
            column.Item().Text(text =>
            {
                text.Span("Full Description: ").Bold();
                text.Span(ticket.Description);
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // --- 2. AI INTELLIGENCE ---
            column.Item().PaddingBottom(2).Text("--- 2. AI INTELLIGENCE ---").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
            
            column.Item().Background(Colors.Blue.Lighten5).Padding(10).Column(aiBox =>
            {
                aiBox.Spacing(4);
                aiBox.Item().Text(text =>
                {
                    text.Span("🔹 AI Summary: ").Bold();
                    text.Span(ticket.AiSummary ?? "N/A");
                });
                aiBox.Item().Text(text =>
                {
                    text.Span("🔹 AI Sentiment: ").Bold();
                    text.Span(ticket.AiSentiment ?? "N/A");
                });
                aiBox.Item().Text(text =>
                {
                    text.Span("🔹 Priority Assigned: ").Bold();
                    text.Span(ticket.AiCategory ?? "High");
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // --- 3. AGENT RESOLUTION & RAG DRAFT ---
            column.Item().PaddingBottom(2).Text("--- 3. AGENT RESOLUTION & RAG DRAFT ---").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
            
            // RAG Draft Reply (Ensure ticket model has RagDraftReply property)
            string draft = string.IsNullOrEmpty(ticket.RagDraftReply) ? "No AI Draft Generated." : ticket.RagDraftReply;
            column.Item().Text("Draft Reply Used:").Bold();
            column.Item().PaddingBottom(5).Text($"\"{draft}\"").Italic().FontColor(Colors.Grey.Darken3);

            // Agent Internal Note (Fetching the latest note)
            var latestNote = ticket.Notes?.OrderByDescending(n => n.CreatedAt).FirstOrDefault();
            string noteText = latestNote != null ? latestNote.Note : "No internal notes added.";
            
            column.Item().PaddingTop(5).Text("Agent Note:").Bold();
            column.Item().Text(noteText);
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