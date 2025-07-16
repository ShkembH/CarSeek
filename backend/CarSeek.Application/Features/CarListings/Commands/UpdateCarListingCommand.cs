using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.CarListings.Commands;

public record UpdateCarListingCommand(
    Guid Id,
    UpdateCarListingRequest Request) : IRequest<CarListingDto>;

public class UpdateCarListingCommandHandler : IRequestHandler<UpdateCarListingCommand, CarListingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public UpdateCarListingCommandHandler(
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

    public async Task<CarListingDto> Handle(UpdateCarListingCommand request, CancellationToken cancellationToken)
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

        // Check if user owns this listing (either directly or through dealership)
        if (carListing.UserId != userId && carListing.Dealership?.UserId != userId)
        {
            throw new ForbiddenAccessException("Only the owner of the listing can update it");
        }

        carListing.Title = request.Request.Title;
        carListing.Description = request.Request.Description;
        carListing.Year = request.Request.Year;
        carListing.Make = request.Request.Make;
        carListing.Model = request.Request.Model;
        carListing.Price = request.Request.Price;
        carListing.Mileage = request.Request.Mileage;

        await _context.SaveChangesAsync(cancellationToken);

        // Log the activity
        await _activityLogger.LogActivityAsync(
            $"Updated car listing: {carListing.Title}",
            ActivityType.ListingUpdated,
            userId,
            carListing.Id,
            "CarListing"
        );

        return _mapper.Map<CarListingDto>(carListing);
    }
}
