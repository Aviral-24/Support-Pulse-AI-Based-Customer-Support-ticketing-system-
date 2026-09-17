using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        // Helper: Fresh In-Memory DB (Purane TestDbContext ka use kar rahe hain)
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetDashboardAnalytics_ReturnsCorrectAggregatedData()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);

            // Seed Database with test tickets (Alag-alag scenarios cover karne ke liye)
            context.Tickets.AddRange(
                new Ticket { Title = "T1", Category = "Hardware", Status = "Open", AiSentiment = "Positive", CreatedAt = DateTime.UtcNow },
                new Ticket { Title = "T2", Category = "Hardware", Status = "Open", AiSentiment = "Positive", CreatedAt = DateTime.UtcNow },
                new Ticket { Title = "T3", Category = "Hardware", Status = "Resolved", AiSentiment = "Pending", CreatedAt = DateTime.UtcNow }, // Pending sentiment (Should be ignored in stats)
                new Ticket { Title = "T4", Category = "Software", Status = "Resolved", AiSentiment = "Negative", CreatedAt = DateTime.UtcNow },
                new Ticket { Title = "T5", Category = "Software", Status = "In Progress", AiSentiment = "N/A", CreatedAt = DateTime.UtcNow }, // N/A sentiment (Should be ignored)
                new Ticket { Title = "T6", Category = "Billing", Status = "Open", AiSentiment = "Neutral", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var controller = new AnalyticsController(context);

            // 🟢 ACT
            var result = await controller.GetDashboardAnalytics();

            // 🟢 ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dashboardData = Assert.IsType<AnalyticsDashboardDto>(okResult.Value);

            // 1. Basic Counts Check
            Assert.Equal(6, dashboardData.TotalTickets);
            Assert.Equal(3, dashboardData.OpenTickets); // T1, T2, T6
            Assert.Equal(2, dashboardData.ResolvedTickets); // T3, T4

            // 2. Category Stats Check
            Assert.Equal(3, dashboardData.TicketsByCategory["Hardware"]);
            Assert.Equal(2, dashboardData.TicketsByCategory["Software"]);
            Assert.Equal(1, dashboardData.TicketsByCategory["Billing"]);

            // 3. Sentiment Stats Check (Pending aur N/A ignore hone chahiye!)
            Assert.Equal(2, dashboardData.TicketsBySentiment["Positive"]);
            Assert.Equal(1, dashboardData.TicketsBySentiment["Negative"]);
            Assert.Equal(1, dashboardData.TicketsBySentiment["Neutral"]);
            Assert.False(dashboardData.TicketsBySentiment.ContainsKey("Pending")); // Shouldn't exist
            Assert.False(dashboardData.TicketsBySentiment.ContainsKey("N/A")); // Shouldn't exist
        }
    }
}