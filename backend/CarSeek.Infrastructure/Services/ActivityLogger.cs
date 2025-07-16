using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;
using System.Threading;

namespace CarSeek.Infrastructure.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IApplicationDbContext _context;

    public ActivityLogger(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(string message, ActivityType type, Guid? userId = null, Guid? entityId = null, string entityType = "")
    {
        var activityLog = new ActivityLog
        {
            Message = message,
            Type = type,
            UserId = userId,
            EntityId = entityId,
            EntityType = entityType,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
