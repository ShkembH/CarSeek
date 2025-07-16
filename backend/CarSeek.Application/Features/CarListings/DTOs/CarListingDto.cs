using CarSeek.Domain.Enums;
using CarSeek.Application.Features.Dealerships.DTOs;

namespace CarSeek.Application.Features.CarListings.DTOs;

public class CarListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public ListingStatus Status { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? Color { get; set; }
    public string? Features { get; set; } // JSON string of selected features
    public DealershipDto? Dealership { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public IEnumerable<CarImageDto> Images { get; set; } = new List<CarImageDto>();
    public string? PrimaryImageUrl { get; set; } // Convenience property for the main image
    public Guid UserId { get; set; } // Add this for chat/ownerId
    public DateTime CreatedAt { get; set; } // Add this for dashboard display
}
