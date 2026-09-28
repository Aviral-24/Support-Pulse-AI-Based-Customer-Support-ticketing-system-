using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateTicketDto
{
  // Strictly check karega ki empty ya null na ho, aur length limits cross na ho
    [Required(ErrorMessage = "Title is required and cannot be empty.", AllowEmptyStrings = false)]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.", AllowEmptyStrings = false)]
    [MinLength(5, ErrorMessage = "Description must be at least 5 characters long.")]
    [MaxLength(2000, ErrorMessage = "Description is too long.")]
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