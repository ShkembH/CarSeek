using CarSeek.Domain.Common;
using CarSeek.Domain.Enums;

namespace CarSeek.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Dealership? Dealership { get; set; }
    public ICollection<SavedListing> SavedListings { get; set; } = new List<SavedListing>();
}
