using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.Common;

public record AuthResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string Token,
    bool RequiresApproval = false,
    string? ApprovalMessage = null);
