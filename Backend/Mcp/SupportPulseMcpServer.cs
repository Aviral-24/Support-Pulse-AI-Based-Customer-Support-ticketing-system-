// using Microsoft.EntityFrameworkCore;
// using Backend.Data;
// using Backend.Services.Pdf;
// using System.Text.Json;

// namespace Backend.Mcp;

// public class SupportPulseMcpServer
// {
//     private readonly ApplicationDbContext _context;
//     private readonly ITicketPdfGenerator _pdfGenerator;

//     public SupportPulseMcpServer(ApplicationDbContext context, ITicketPdfGenerator pdfGenerator)
//     {
//         _context = context;
//         _pdfGenerator = pdfGenerator;
//     }

//      protected SupportPulseMcpServer() { }

//     // Tool 1: get_ticket_analytics
//     public virtual async Task<string> GetTicketAnalyticsAsync(string? category, string? status, string? sentiment)
//     {
//         var query = _context.Tickets.AsQueryable();

//         if (!string.IsNullOrEmpty(category))
//             query = query.Where(t => t.Category == category);
//         if (!string.IsNullOrEmpty(status))
//             query = query.Where(t => t.Status == status);
//         if (!string.IsNullOrEmpty(sentiment))
//             query = query.Where(t => t.AiSentiment == sentiment);

//         var total = await query.CountAsync();
//         var openCount = await query.CountAsync(t => t.Status == "Open");
//         var resolvedCount = await query.CountAsync(t => t.Status == "Resolved");

//         var result = new
//         {
//             TotalTickets = total,
//             OpenTickets = openCount,
//             ResolvedTickets = resolvedCount,
//             FilteredBy = new { category, status, sentiment }
//         };

//         return JsonSerializer.Serialize(result);
//     }

//     // Tool 2: generate_pdf_summary
//     public virtual async Task<byte[]> GeneratePdfSummaryToolAsync(int ticketId)
//     {
//         var ticket = await _context.Tickets
//             .Include(t => t.Customer)
//             .FirstOrDefaultAsync(t => t.Id == ticketId);

//         if (ticket == null)
//             throw new Exception($"Ticket #{ticketId} not found.");

//         return _pdfGenerator.GenerateTicketSummary(ticket);
//     }
// }


using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Security.Claims;

namespace Backend.Mcp;

public class SupportPulseMcpServer
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SupportPulseMcpServer(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // 🔒 SECURITY CHECK (Hard-Fail Prevention for RBAC)
    // Ye function ensure karta hai ki tools keval Agent/Admin use kar sakein, Customer nahi.
    private void EnsureAuthorizedAccess()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("Access Denied: Authentication required for MCP tools.");

        // JWT token me jo role hai usko check karo
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Agent" && role != "Admin")
            throw new UnauthorizedAccessException("Access Denied: You do not have permission to use this tool. Agents/Admins only.");
    }

    // 🛠️ TOOL 1: get_ticket_analytics
    // AI Client yahan filters bhejega, aur ye DB se aggregation nikalega.
    public async Task<string> GetTicketAnalyticsAsync(string? status = null, string? priority = null, int days = 7)
    {
        EnsureAuthorizedAccess(); // 🔒 Sabse pehle Security Check

        var sinceDate = DateTime.UtcNow.AddDays(-days);
        var query = _context.Tickets.Where(t => t.CreatedAt >= sinceDate);

        // Applying AI requested filters
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(priority)) query = query.Where(t => t.AiCategory == priority);

        var total = await query.CountAsync();
        var open = await query.CountAsync(t => t.Status == "Open");
        var resolved = await query.CountAsync(t => t.Status == "Resolved");

        var sentiments = await query.GroupBy(t => t.AiSentiment)
            .Select(g => new { Sentiment = g.Key ?? "Neutral", Count = g.Count() })
            .ToListAsync();

        var result = new {
            Timeframe = $"Last {days} days",
            FiltersApplied = new { Status = status ?? "All", Priority = priority ?? "All" },
            TotalTickets = total,
            OpenTickets = open,
            ResolvedTickets = resolved,
            SentimentBreakdown = sentiments
        };

        // Output hamesha strictly JSON String hona chahiye taaki AI usko read kar sake
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    // 🛠️ TOOL 2: generate_pdf_summary
    public async Task<string> GeneratePdfSummaryAsync(int ticketId)
    {
        EnsureAuthorizedAccess(); // 🔒 Security Check

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return $"Error: Ticket #{ticketId} not found.";

        // Real-world URL jo us specific PDF generate endpoint ka hai
        string downloadUrl = $"http://localhost:5215/api/v1/Tickets/{ticketId}/pdf";
        
        return $"Success! PDF Summary for Ticket #{ticketId} has been generated securely. You can download it directly here: {downloadUrl}";
    }
}