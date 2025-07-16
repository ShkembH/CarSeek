using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.Common;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Country,
    string? City,
    UserRole Role,
    string? CompanyName,
    string? CompanyUniqueNumber,
    string? Location,
    string? BusinessCertificatePath);
