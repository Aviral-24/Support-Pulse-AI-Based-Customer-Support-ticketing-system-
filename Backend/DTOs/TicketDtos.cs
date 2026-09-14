using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateTicketDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";

    // File uploads ke liye properties add karo
    public IFormFile? ImageFile { get; set; }
    public IFormFile? AudioFile { get; set; }
}


public class UpdateTicketStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // Open, In Progress, Resolved
}

public class AddNoteDto
{
    [Required]
    public string Note { get; set; } = string.Empty;
}