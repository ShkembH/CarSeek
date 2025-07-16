namespace CarSeek.Application.Features.Admin.DTOs;

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalListings { get; set; }
    public int TotalDealerships { get; set; }
    public int PendingApprovals { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ActiveUsers { get; set; }
}
