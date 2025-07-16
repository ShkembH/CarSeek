using CarSeek.Domain.Common;
using CarSeek.Domain.ValueObjects;

namespace CarSeek.Domain.Entities;

public class Dealership : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string CompanyUniqueNumber { get; set; } = string.Empty;
    public string BusinessCertificatePath { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    // Navigation properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<CarListing> Listings { get; set; } = new List<CarListing>();
}
