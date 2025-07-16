using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Auth.Commands;

public record RegisterCommand(
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
    string? BusinessCertificatePath
) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IActivityLogger _activityLogger;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IActivityLogger activityLogger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _activityLogger = activityLogger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new DuplicateEmailException(request.Email);
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Country = request.Country,
            City = request.City,
            Role = request.Role,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // If the user is a dealership, create a Dealership record
        if (user.Role == UserRole.Dealership)
        {
            var dealership = new Dealership
            {
                Name = request.CompanyName ?? string.Empty,
                Description = "Auto-created dealership. Please update your profile.",
                Address = new CarSeek.Domain.ValueObjects.Address
                {
                    Street = user.City ?? "",
                    City = user.City ?? "",
                    State = user.Country ?? "",
                    PostalCode = "",
                    Country = user.Country ?? ""
                },
                PhoneNumber = user.PhoneNumber ?? "",
                Website = "",
                CompanyUniqueNumber = request.CompanyUniqueNumber ?? string.Empty,
                BusinessCertificatePath = request.BusinessCertificatePath ?? string.Empty,
                Location = request.Location ?? string.Empty,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Dealerships.Add(dealership);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Log the activity
        await _activityLogger.LogActivityAsync(
            $"New user registered: {user.FirstName} {user.LastName} ({user.Email})",
            ActivityType.UserRegistration,
            user.Id,
            user.Id,
            "User"
        );

        // Generate token
        var token = _tokenGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token);
    }
}
