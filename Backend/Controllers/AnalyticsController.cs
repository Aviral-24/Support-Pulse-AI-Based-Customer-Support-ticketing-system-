using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Agent/Admin access ke liye (roles abhi hata diye hain testing smooth rakhne ke liye)
public class AnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/v1/Analytics/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        // 1. Basic Counts
        var totalTickets = await _context.Tickets.CountAsync();
        var openTickets = await _context.Tickets.CountAsync(t => t.Status == "Open");
        var resolvedTickets = await _context.Tickets.CountAsync(t => t.Status == "Resolved");

        // 2. AI Sentiment Breakdown (Real-time DB Aggregation)
        var sentimentStats = await _context.Tickets
            .Where(t => t.AiSentiment != null && t.AiSentiment != "N/A" && t.AiSentiment != "Pending")
            .GroupBy(t => t.AiSentiment)
            .Select(g => new { Sentiment = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Sentiment!, v => v.Count);

        // 3. Category Breakdown
        var categoryStats = await _context.Tickets
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Category, v => v.Count);

        // 4. Wrap up data in DTO
        var analyticsData = new AnalyticsDashboardDto
        {
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            ResolvedTickets = resolvedTickets,
            TicketsBySentiment = sentimentStats,
            TicketsByCategory = categoryStats
        };

        return Ok(analyticsData);
    }
}