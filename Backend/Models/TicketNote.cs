namespace Backend.Models;

public class TicketNote
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public int AgentId { get; set; }

    public User? Agent { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}