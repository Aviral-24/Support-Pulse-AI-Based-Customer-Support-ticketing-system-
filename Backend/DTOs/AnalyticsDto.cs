namespace Backend.DTOs;

public class AnalyticsDashboardDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedTickets { get; set; }
    
    // Charts ke liye Dictionary format best rehta hai (e.g., "Happy": 5, "Angry": 2)
    public Dictionary<string, int> TicketsBySentiment { get; set; } = new();
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
}