using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services.Pdf;
using System.Text.Json;

namespace Backend.Mcp;

public class SupportPulseMcpServer
{
    private readonly ApplicationDbContext _context;
    private readonly ITicketPdfGenerator _pdfGenerator;

    public SupportPulseMcpServer(ApplicationDbContext context, ITicketPdfGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    // Tool 1: get_ticket_analytics
    public async Task<string> GetTicketAnalyticsAsync(string? category, string? status, string? sentiment)
    {
        var query = _context.Tickets.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(sentiment))
            query = query.Where(t => t.AiSentiment == sentiment);

        var total = await query.CountAsync();
        var openCount = await query.CountAsync(t => t.Status == "Open");
        var resolvedCount = await query.CountAsync(t => t.Status == "Resolved");

        var result = new
        {
            TotalTickets = total,
            OpenTickets = openCount,
            ResolvedTickets = resolvedCount,
            FilteredBy = new { category, status, sentiment }
        };

        return JsonSerializer.Serialize(result);
    }

    // Tool 2: generate_pdf_summary
    public async Task<byte[]> GeneratePdfSummaryToolAsync(int ticketId)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new Exception($"Ticket #{ticketId} not found.");

        return _pdfGenerator.GenerateTicketSummary(ticket);
    }
}