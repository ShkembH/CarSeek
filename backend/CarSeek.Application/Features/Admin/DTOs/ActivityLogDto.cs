using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Admin.DTOs;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public string TypeName => Type.ToString();
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string TimeAgo => GetTimeAgo(CreatedAt);
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalHours < 1)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalDays < 1)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";

        return dateTime.ToString("MMM dd, yyyy");
    }
}
