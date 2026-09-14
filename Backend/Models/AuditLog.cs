namespace Backend.Models;

public class AuditLog
{
    public int Id { get; set; }
    
    public int UserId { get; set; } // Kisne action kiya
    
    public string Action { get; set; } = string.Empty; // Kya action kiya (e.g., "Downloaded PDF", "Updated Status")
    
    public string EntityType { get; set; } = string.Empty; // Kis cheez par action kiya (e.g., "Ticket")
    
    public int? EntityId { get; set; } // Ticket ka ID
    
    public string? Details { get; set; } // Extra info
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Kab kiya
}