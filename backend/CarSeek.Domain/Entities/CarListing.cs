using CarSeek.Domain.Common;
using CarSeek.Domain.Enums;

namespace CarSeek.Domain.Entities;

public class CarListing : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }  // Removed semicolon
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal Price { get; set; }  // Removed semicolon
    public int Mileage { get; set; }  // Removed semicolon
    public ListingStatus Status { get; set; }  // Removed semicolon
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? Color { get; set; }
    public string? Features { get; set; } // JSON string of selected features

    // Navigation properties
    public Guid? DealershipId { get; set; }  // Made optional
    public Dealership? Dealership { get; set; }  // Made optional

    // Add UserId for individual users
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<SavedListing> SavedByUsers { get; set; } = new List<SavedListing>();
    public ICollection<CarImage> Images { get; set; } = new List<CarImage>();
}
