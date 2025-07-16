namespace CarSeek.Application.Features.Auth.Common;

public record LoginRequest(
    string Email,
    string Password);
