namespace CarSeek.Application.Features.Admin.DTOs;

public class UpdateUserRequest
{
    // Common fields
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool? IsActive { get; set; }

    // Individual user fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Dealership fields
    public string? CompanyName { get; set; }
    public string? CompanyUniqueNumber { get; set; }
    public string? Location { get; set; }
    public string? DealershipPhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
} 