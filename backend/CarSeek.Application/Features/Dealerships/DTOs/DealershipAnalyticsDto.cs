namespace CarSeek.Application.Features.Dealerships.DTOs;

public class DealershipAnalyticsDto
{
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int PendingListings { get; set; }
    public int SoldListings { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public int ViewsThisMonth { get; set; }
    public int InquiriesThisMonth { get; set; }
    public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
}

public class MonthlyStatsDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int ListingsCreated { get; set; }
    public int ListingsSold { get; set; }
    public decimal Revenue { get; set; }
}
