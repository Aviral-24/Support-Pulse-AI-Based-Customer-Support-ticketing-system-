using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Backend.Controllers;
using Backend.Models;
using Backend.DTOs;
using Xunit;

namespace Units_Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // 1. ARRANGE: In-Memory DB aur fake JWT config setup karna
        var options = new DbContextOptionsBuilder<Backend.Data.ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Hum wahi TestApplicationDbContext use karenge jo pichle test me banaya tha (Vector fix wala)
        using var context = new TestApplicationDbContext(options);

        // Database me ek real user daal rahe hain jiska password hashed hai
        var plainPassword = "MySecretPassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, 8);
        
        context.Users.Add(new User 
        { 
            Name = "Test User", 
            Email = "test@secure.com", 
            PasswordHash = hashedPassword, 
            Role = "Customer" 
        });
        await context.SaveChangesAsync();

        // IConfiguration ko mock karna kyunki AuthController usko JWT banane ke liye use karta hai
        var inMemorySettings = new Dictionary<string, string?> 
        {
            {"Jwt:Key", "SuperSecretKeyForTestingTheAuthApi123"}, // Minimum 16-32 chars key for HMAC
            {"Jwt:Issuer", "SupportPulseTest"},
            {"Jwt:Audience", "SupportPulseTest"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var controller = new AuthController(context, configuration);

        // Ek login request banana jisme email sahi hai par PASSWORD GALAT hai
        var loginRequest = new LoginDto 
        { 
            Email = "test@secure.com", 
            Password = "WrongPassword999" // Hacker attempt
        };

        // 2. ACT: Login API ko call karna
        var result = await controller.Login(loginRequest);

        // 3. ASSERT: Verify karna ki system ne strict "Unauthorized" diya
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
    }
}