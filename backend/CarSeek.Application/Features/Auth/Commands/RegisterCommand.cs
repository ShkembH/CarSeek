using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;
using Microsoft.AspNetCore.Http;

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
    IFormFile? BusinessCertificate
) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IActivityLogger _activityLogger;
    private readonly IFileStorageService _fileStorageService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IActivityLogger activityLogger,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _activityLogger = activityLogger;
        _fileStorageService = fileStorageService;
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
            Status = request.Role == UserRole.Dealership ? UserStatus.PendingApproval : UserStatus.Approved,
            IsActive = request.Role == UserRole.Dealership ? false : true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // If the user is a dealership, create a Dealership record
        if (user.Role == UserRole.Dealership)
        {
            string businessCertificatePath = string.Empty;
            
            // Handle business certificate file upload
            if (request.BusinessCertificate != null && request.BusinessCertificate.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await request.BusinessCertificate.CopyToAsync(memoryStream, cancellationToken);
                var fileData = memoryStream.ToArray();
                
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{request.BusinessCertificate.FileName}";
                businessCertificatePath = await _fileStorageService.UploadFileAsync(
                    fileName, 
                    fileData, 
                    "business-certificates", 
                    cancellationToken);
            }

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
                BusinessCertificatePath = businessCertificatePath,
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

        // Determine if approval is required
        bool requiresApproval = user.Role == UserRole.Dealership;
        string? approvalMessage = requiresApproval 
            ? "Your dealership account has been created successfully and is pending admin approval. You will be able to access your account once approved." 
            : null;

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token,
            requiresApproval,
            approvalMessage);
    }
}
