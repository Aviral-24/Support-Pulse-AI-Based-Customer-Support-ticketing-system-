namespace Backend.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Day 1: Temporary field.
    // Day 2 me proper BCrypt/Argon2 password hashing implement karenge.
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}