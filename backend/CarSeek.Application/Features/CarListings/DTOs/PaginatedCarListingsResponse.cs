namespace CarSeek.Application.Features.CarListings.DTOs;

public class PaginatedCarListingsResponse
{
    public IEnumerable<CarListingDto> Items { get; set; } = new List<CarListingDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}
