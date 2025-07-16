namespace CarSeek.Application.Common.Models;

public class ProfileResponse
{
    // Common fields
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string Role { get; set; } = string.Empty;

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Dealership fields
    public string? CompanyName { get; set; }
    public string? CompanyUniqueNumber { get; set; }
    public string? BusinessCertificatePath { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
}
