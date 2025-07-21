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
        // Find user by email (case insensitive)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        // Check if user exists and verify password
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            if (user.Role == UserRole.Dealership)
            {
                throw new AuthenticationException("Your dealership account is pending approval. Please wait for admin approval before logging in.");
            }
            else
            {
                throw new AuthenticationException("Your account has been deactivated. Please contact support.");
            }
        }

        // For dealership users, check if they're approved
        if (user.Role == UserRole.Dealership)
        {
            var dealership = await _context.Dealerships
                .FirstOrDefaultAsync(d => d.UserId == user.Id, cancellationToken);
            
            if (dealership == null || !dealership.IsApproved)
            {
                throw new AuthenticationException("Your dealership account is pending approval. Please wait for admin approval before logging in.");
            }
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
