using Backend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Backend.Data;

public static class DataSeeder
{
    // Extension method to clean up Program.cs
    public static void SeedSuperAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var superAdmin = dbContext.Users.FirstOrDefault(u => u.Email == "admin@supportpulse.com");
        
        if (superAdmin == null)
        {
            superAdmin = new User
            {
                Name = "Super Admin",
                Email = "admin@supportpulse.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                Role = "Admin"
            };
            dbContext.Users.Add(superAdmin);
        }
        else
        {
            // Update ensures it's always fixed on restart if someone messes with it
            superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");
            superAdmin.Role = "Admin";
            dbContext.Users.Update(superAdmin);
        }
        
        dbContext.SaveChanges();
    }
}