using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public UserRole Role { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyUniqueNumber { get; set; }
    public string? Location { get; set; }
    public string? BusinessCertificatePath { get; set; }
}
