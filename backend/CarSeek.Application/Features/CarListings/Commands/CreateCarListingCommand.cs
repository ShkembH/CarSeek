using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Features.CarListings.DTOs;
using System;

namespace CarSeek.Application.Features.CarListings.Commands;

public record CreateCarListingCommand(CreateCarListingRequest Request) : IRequest<CarListingDto>;

public class CreateCarListingCommandHandler : IRequestHandler<CreateCarListingCommand, CarListingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public CreateCarListingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _activityLogger = activityLogger;
    }

    public async Task<CarListingDto> Handle(CreateCarListingCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create a listing");
        }

        var userId = _currentUserService.UserId.Value;

        // Get the current user to check their role
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Debug logging
        Console.WriteLine($"[DEBUG] User ID: {userId}");
        Console.WriteLine($"[DEBUG] User Role: {currentUser.Role}");
        Console.WriteLine($"[DEBUG] User Role Value: {(int)currentUser.Role}");
        Console.WriteLine($"[DEBUG] User Email: {currentUser.Email}");

        // Check if user has a dealership (optional)
        var dealership = await _context.Dealerships
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        Console.WriteLine($"[DEBUG] Has Dealership: {dealership != null}");
        if (dealership != null)
        {
            Console.WriteLine($"[DEBUG] Dealership Name: {dealership.Name}");
        }

        // Determine status based on user role and dealership status
        // Admin and Dealership users get auto-approved, regular users need approval
        var listingStatus = (currentUser.Role == UserRole.Admin || currentUser.Role == UserRole.Dealership)
            ? ListingStatus.Active
            : ListingStatus.Pending;

        Console.WriteLine($"[DEBUG] Listing Status: {listingStatus}");
        Console.WriteLine($"[DEBUG] Is Auto-Approved: {listingStatus == ListingStatus.Active}");
        Console.WriteLine($"[DEBUG] Features received: {request.Request.Features}");

        var carListing = new CarListing
        {
            Title = request.Request.Title,
            Description = request.Request.Description,
            Year = request.Request.Year,
            Make = request.Request.Make,
            Model = request.Request.Model,
            Price = request.Request.Price,
            Mileage = request.Request.Mileage,
            FuelType = request.Request.FuelType,
            Transmission = request.Request.Transmission,
            Color = request.Request.Color,
            Features = request.Request.Features, // Save the selected features
            Status = listingStatus, // Use dynamic status based on user type
            UserId = userId,  // Use the non-nullable value
            DealershipId = dealership?.Id,  // Set dealership ID if user has one, otherwise null
            CreatedAt = DateTime.UtcNow // Set creation date
        };

        Console.WriteLine($"[DEBUG] Features saved to car listing: {carListing.Features}");

        _context.CarListings.Add(carListing);
        await _context.SaveChangesAsync(cancellationToken);

        // Log the activity with status information
        var statusMessage = listingStatus == ListingStatus.Active
            ? $"Created new car listing (auto-approved): {carListing.Title}"
            : $"Created new car listing (pending approval): {carListing.Title}";

        await _activityLogger.LogActivityAsync(
            statusMessage,
            ActivityType.ListingCreated,
            userId,
            carListing.Id,
            "CarListing"
        );

        return _mapper.Map<CarListingDto>(carListing);
    }
}
