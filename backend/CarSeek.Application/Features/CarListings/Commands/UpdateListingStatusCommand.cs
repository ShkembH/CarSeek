using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using CarSeek.Domain.Entities;

namespace CarSeek.Application.Features.CarListings.Commands;

public record UpdateListingStatusCommand(
    Guid Id,
    UpdateListingStatusRequest Request) : IRequest<CarListingDto>;

public class UpdateListingStatusCommandHandler : IRequestHandler<UpdateListingStatusCommand, CarListingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public UpdateListingStatusCommandHandler(
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

    public async Task<CarListingDto> Handle(UpdateListingStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var carListing = await _context.CarListings
            .Include(c => c.Dealership)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (carListing == null)
        {
            throw new NotFoundException(nameof(CarListing), request.Id);
        }

        // Check if user is admin or owns this listing
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwner = carListing.UserId == userId || carListing.Dealership?.UserId == userId;

        if (!isAdmin && !isOwner)
        {
            throw new ForbiddenAccessException("Only the owner of the listing or an admin can update its status");
        }

        // Remove the conflicting check - admins should be able to approve any listing
        // if (carListing.Dealership.UserId != _currentUserService.UserId)
        // {
        //     throw new ForbiddenAccessException("Only the dealership that created the listing can update its status");
        // }

        // Validate status transitions
        ValidateStatusTransition(carListing.Status, request.Request.Status);

        // Add logging before and after status change
        var oldStatus = carListing.Status;
        carListing.Status = request.Request.Status;

        // Log the change
        Console.WriteLine($"Updating listing {carListing.Id} from {oldStatus} to {carListing.Status}");

        await _context.SaveChangesAsync(cancellationToken);

        // Log the activity
        var activityType = request.Request.Status == ListingStatus.Active ? ActivityType.ListingApproved : ActivityType.ListingRejected;
        var message = request.Request.Status == ListingStatus.Active
            ? $"Approved car listing: {carListing.Title}"
            : $"Rejected car listing: {carListing.Title}";

        await _activityLogger.LogActivityAsync(
            message,
            activityType,
            userId,
            carListing.Id,
            "CarListing"
        );

        // Verify the change was saved
        var updatedListing = await _context.CarListings.FindAsync(carListing.Id);
        Console.WriteLine($"After save: Listing {updatedListing.Id} status is {updatedListing.Status}");

        return _mapper.Map<CarListingDto>(carListing);
    }

    private void ValidateStatusTransition(ListingStatus currentStatus, ListingStatus newStatus)
    {
        var invalidTransitions = new[]
        {
            (ListingStatus.Rejected, ListingStatus.Active),
            (ListingStatus.Sold, ListingStatus.Active),
            (ListingStatus.Sold, ListingStatus.Inactive),
            (ListingStatus.Rejected, ListingStatus.Inactive)
        };

        if (invalidTransitions.Contains((currentStatus, newStatus)))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Status", new[] { $"Cannot change status from {currentStatus} to {newStatus}" } }
            };
            throw new ValidationException(errors);
        }
    }
}
