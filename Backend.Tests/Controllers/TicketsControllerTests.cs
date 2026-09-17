using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Services;
using Backend.Services.Async;

namespace Backend.Tests.Controllers
{
    public class TicketsControllerTests
    {
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly Mock<ITicketQueue> _mockTicketQueue;
        private readonly Mock<IAuditService> _mockAuditService;

        public TicketsControllerTests()
        {
            // 🟢 ARRANGE: Mocks setup for external services
            _mockStorageService = new Mock<IStorageService>();
            _mockTicketQueue = new Mock<ITicketQueue>();
            _mockAuditService = new Mock<IAuditService>();
        }

        // Helper: In-Memory DB options (Reusing logic from Auth tests)
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        // Helper: Controller setup with Fake User Token (Claims)
        private TicketsController CreateControllerWithFakeUser(TestDbContext context, int userId, string role)
        {
            var controller = new TicketsController(context, _mockStorageService.Object, _mockTicketQueue.Object);
            
            // Creating a Fake User Token
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task CreateTicket_ValidData_SavesToDbAndQueuesForAi()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            
            // Logged in as Customer ID 10
            var controller = CreateControllerWithFakeUser(context, userId: 10, role: "Customer");

            var request = new CreateTicketDto
            {
                Title = "Router not working",
                Description = "Red light is blinking continuously",
                Category = "Hardware"
            };

            // 🟢 ACT
            var result = await controller.CreateTicket(request);

            // 🟢 ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Verify DB Save
            var savedTicket = await context.Tickets.FirstOrDefaultAsync();
            Assert.NotNull(savedTicket);
            Assert.Equal("Router not working", savedTicket.Title);
            Assert.Equal(10, savedTicket.CustomerId);
            Assert.Equal("Open", savedTicket.Status);

            // Verify Queue was called exactly ONCE so AI can process it
            _mockTicketQueue.Verify(q => q.EnqueueTicketAsync(savedTicket.Id), Times.Once);
        }

        [Fact]
        public async Task GetMyTickets_ReturnsOnlyTicketsForLoggedInCustomer()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);

            // Seed DB with mixed tickets
            context.Tickets.AddRange(
                new Ticket { Title = "T1", CustomerId = 1, Status = "Open", CreatedAt = DateTime.UtcNow },
                new Ticket { Title = "T2", CustomerId = 1, Status = "Resolved", CreatedAt = DateTime.UtcNow },
                new Ticket { Title = "T3", CustomerId = 2, Status = "Open", CreatedAt = DateTime.UtcNow } // Other customer
            );
            await context.SaveChangesAsync();

            // Logged in as Customer ID 1
            var controller = CreateControllerWithFakeUser(context, userId: 1, role: "Customer");

            // 🟢 ACT
            var result = await controller.GetMyTickets();

            // 🟢 ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            // We expect a List of anonymous objects, we can cast it to IEnumerable<object> or dynamic
            var tickets = okResult.Value as IEnumerable<object>;
            
            Assert.NotNull(tickets);
            Assert.Equal(2, tickets.Count()); // Should only return 2 tickets for Customer ID 1
        }

        [Fact]
        public async Task UpdateStatus_ValidStatus_UpdatesDbAndLogsAudit()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);

            var ticket = new Ticket { Title = "Test Ticket", CustomerId = 1, Status = "Open" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Logged in as Agent ID 99
            var controller = CreateControllerWithFakeUser(context, userId: 99, role: "Agent");
            var request = new UpdateTicketStatusDto { Status = "Resolved" };

            // 🟢 ACT
            // Passing the Mocked Audit Service via method injection
            var result = await controller.UpdateStatus(ticket.Id, request, _mockAuditService.Object);

            // 🟢 ASSERT
            Assert.IsType<OkObjectResult>(result);

            // Check if DB updated
            var updatedTicket = await context.Tickets.FindAsync(ticket.Id);
            Assert.Equal("Resolved", updatedTicket!.Status);

            // Check if Audit Service was called to log the action
            _mockAuditService.Verify(a => a.LogActionAsync(
                99, 
                "Changed status to Resolved", 
                "Ticket", 
                ticket.Id, 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);

            var ticket = new Ticket { Title = "Test Ticket", CustomerId = 1, Status = "Open" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var controller = CreateControllerWithFakeUser(context, userId: 99, role: "Agent");
            
            // "Closed" is not in the validStatuses array ["Open", "In Progress", "Resolved"]
            var request = new UpdateTicketStatusDto { Status = "Closed" }; 

            // 🟢 ACT
            var result = await controller.UpdateStatus(ticket.Id, request, _mockAuditService.Object);

            // 🟢 ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid status.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddNote_ValidNote_SavesToDb()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);

            var ticket = new Ticket { Title = "Test Ticket", CustomerId = 1, Status = "Open" };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Logged in as Agent ID 99
            var controller = CreateControllerWithFakeUser(context, userId: 99, role: "Agent");
            var request = new AddNoteDto { Note = "Contacted customer, waiting for reply." };

            // 🟢 ACT
            var result = await controller.AddNote(ticket.Id, request);

            // 🟢 ASSERT
            Assert.IsType<OkObjectResult>(result);

            var savedNote = await context.TicketNotes.FirstOrDefaultAsync();
            Assert.NotNull(savedNote);
            Assert.Equal(ticket.Id, savedNote.TicketId);
            Assert.Equal(99, savedNote.AgentId);
            Assert.Equal("Contacted customer, waiting for reply.", savedNote.Note);
        }
    }
}