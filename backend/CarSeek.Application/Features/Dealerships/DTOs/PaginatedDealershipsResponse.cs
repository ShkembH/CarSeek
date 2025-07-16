namespace CarSeek.Application.Features.Dealerships.DTOs;

public class PaginatedDealershipsResponse
{
    public IEnumerable<DealershipDto> Items { get; set; } = new List<DealershipDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}
