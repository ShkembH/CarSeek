using CarSeek.Domain.Common;
using CarSeek.Domain.Enums;

namespace CarSeek.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
}
