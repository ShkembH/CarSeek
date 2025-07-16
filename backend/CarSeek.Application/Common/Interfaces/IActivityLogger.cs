using CarSeek.Domain.Enums;

namespace CarSeek.Application.Common.Interfaces;

public interface IActivityLogger
{
    Task LogActivityAsync(string message, ActivityType type, Guid? userId = null, Guid? entityId = null, string entityType = "");
}
