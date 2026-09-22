using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Xunit;

namespace Units_Tests;

// 🔥 SMART FIX: Main code ko chhede bina, sirf Test ke liye ek derived DbContext banaya
public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // InMemory DB ko pgvector samajh nahi aata, isliye usko sirf test me ignore kar rahe hain
        modelBuilder.Entity<Ticket>().Ignore(t => t.Embedding);
    }
}

public class TicketsControllerTests
{
    [Fact]
    public async Task GetMyTickets_BolaProtection_ReturnsOnlyLoggedInCustomerTickets()
    {
        // 1. ARRANGE: Asli DBContext ki jagah humne jo upar Test DBContext banaya hai wo use karenge
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Har test ke liye fresh DB
            .Options;

        using var context = new TestApplicationDbContext(options);

        // Database me 2 alag-alag customers ki tickets daalna
        context.Tickets.Add(new Ticket { Title = "My Ticket", CustomerId = 101, Status = "Open" });
        context.Tickets.Add(new Ticket { Title = "Hacker's Ticket", CustomerId = 999, Status = "Open" });
        await context.SaveChangesAsync();

        // Controller setup karna
        var controller = new TicketsController(context, null!, null!);

        // Fake HttpContext banana jisme User ID "101" login hai
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "101") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // 2. ACT: API call karna
        var result = await controller.GetMyTickets() as OkObjectResult;

        // 3. ASSERT: Verification
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify karna ki list me sirf 1 ticket aayi hai (doosri nahi aayi)
        var enumerable = result.Value as System.Collections.IEnumerable;
        Assert.NotNull(enumerable);
        
        int ticketCount = 0;
        foreach (var item in enumerable)
        {
            ticketCount++;
            
            // Verify karna ki jo ticket aayi hai uska Title "My Ticket" hi hai
            var titleProperty = item.GetType().GetProperty("Title");
            var title = titleProperty?.GetValue(item, null)?.ToString();
            Assert.Equal("My Ticket", title);
        }

        // Agar count 1 hai, iska matlab ID 999 ki ticket secure hai aur leak nahi hui!
        Assert.Equal(1, ticketCount);
    }
}