using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Domain.Entities; // Add this line
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IActivityLogger _activityLogger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IActivityLogger activityLogger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _activityLogger = activityLogger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Check if user exists and verify password
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        // Log the activity
        await _activityLogger.LogActivityAsync(
            $"User logged in: {user.FirstName} {user.LastName} ({user.Email})",
            ActivityType.UserLogin,
            user.Id,
            user.Id,
            "User"
        );

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token);
    }
}
