using CarSeek.Domain.Common;

namespace CarSeek.Domain.Entities;

public class CarImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public Guid CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;
}
