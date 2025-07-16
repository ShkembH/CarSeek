using Microsoft.AspNetCore.Http;

namespace CarSeek.Application.Common.Models;

public class UpdateProfileRequest
{
    // Common fields
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Dealership fields
    public string? CompanyName { get; set; }
    public string? CompanyUniqueNumber { get; set; }
    public IFormFile? BusinessCertificate { get; set; }
    public string? Location { get; set; }
}
