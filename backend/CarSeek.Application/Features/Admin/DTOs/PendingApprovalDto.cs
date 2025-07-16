namespace CarSeek.Application.Features.Admin.DTOs;

public class PendingApprovalDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Submitter { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
