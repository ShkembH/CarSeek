using CarSeek.Domain.Common;

namespace CarSeek.Domain.Entities;

public class SavedListing : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CarListingId { get; set; }
    public CarListing CarListing { get; set; } = null!;
}
