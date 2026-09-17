using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Backend.Services;
using Backend.Data;
using Backend.Models;

namespace Backend.Tests.Services
{
    // 🔥 NINJA FIX: Vector error se bachne ke liye Services ke liye bhi custom context
    public class ServiceTestDbContext : ApplicationDbContext
    {
        public ServiceTestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<Pgvector.Vector>(); 
        }
    }

    public class AuditServiceTests
    {
        // Helper: Fresh In-Memory DB for every test run
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task LogActionAsync_SavesAuditLogToDatabase()
        {
            // 🟢 ARRANGE
            var options = CreateNewContextOptions();
            using var context = new ServiceTestDbContext(options);
            
            // Injecting the mocked DB context into the actual service
            var auditService = new AuditService(context);

            int testUserId = 99;
            string testAction = "STATUS_UPDATE";
            string testEntityType = "Ticket";
            int testEntityId = 101;
            string testDetails = "Ticket marked as resolved from Agent Dashboard";

            // 🟢 ACT
            await auditService.LogActionAsync(testUserId, testAction, testEntityType, testEntityId, testDetails);

            // 🟢 ASSERT
            // Database me jaakar check karte hain ki entry aayi ya nahi
            var savedLog = await context.AuditLogs.FirstOrDefaultAsync();
            
            Assert.NotNull(savedLog);
            Assert.Equal(testUserId, savedLog.UserId);
            Assert.Equal(testAction, savedLog.Action);
            Assert.Equal(testEntityType, savedLog.EntityType);
            Assert.Equal(testEntityId, savedLog.EntityId);
            Assert.Equal(testDetails, savedLog.Details);
            
            // Timestamp abhi (aaj aur abhi) ki honi chahiye
            Assert.True((DateTime.UtcNow - savedLog.Timestamp).TotalMinutes < 1);
        }
    }
}