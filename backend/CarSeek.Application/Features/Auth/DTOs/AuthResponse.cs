using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.DTOs;

public class AuthResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public AuthResponse(Guid id, string email, string firstName, string lastName, UserRole role, string token)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        Token = token;
    }
}
