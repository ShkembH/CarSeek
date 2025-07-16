using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.CarListings.Commands;

public record DeleteCarListingCommand(Guid Id) : IRequest;

public class DeleteCarListingCommandHandler : IRequestHandler<DeleteCarListingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogger _activityLogger;

    public DeleteCarListingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _activityLogger = activityLogger;
    }

    public async Task Handle(DeleteCarListingCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var userId = _currentUserService.UserId.Value;

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
            throw new ForbiddenAccessException("Only the owner of the listing or an admin can delete it");
        }

        // Log the activity before deleting
        await _activityLogger.LogActivityAsync(
            $"Deleted car listing: {carListing.Title}",
            ActivityType.ListingDeleted,
            userId,
            carListing.Id,
            "CarListing"
        );

        _context.CarListings.Remove(carListing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
