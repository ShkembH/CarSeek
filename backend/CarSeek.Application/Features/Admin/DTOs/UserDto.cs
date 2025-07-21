using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Admin.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyUniqueNumber { get; set; }
    public string? Location { get; set; }
    public string? DealershipPhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? BusinessCertificatePath { get; set; }
    public string? Description { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public bool? IsDealershipApproved { get; set; }
}
