using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public interface IAuditService
{
    Task LogActionAsync(int userId, string action, string entityType, int entityId, string details = "");
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(int userId, string action, string entityType, int entityId, string details = "")
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}