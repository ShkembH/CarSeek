using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;

namespace CarSeek.Application.Features.CarListings.Queries;

public record GetCarListingQuery(Guid Id) : IRequest<CarListingDto>;

public class GetCarListingQueryHandler : IRequestHandler<GetCarListingQuery, CarListingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCarListingQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CarListingDto> Handle(GetCarListingQuery request, CancellationToken cancellationToken)
    {
        var carListing = await _context.CarListings
            .Include(c => c.Dealership)
                .ThenInclude(d => d.User)
            .Include(c => c.User)
            .Include(c => c.Images) // Make sure images are included
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (carListing == null)
        {
            throw new NotFoundException("CarListing", request.Id);
        }

        Console.WriteLine($"[DEBUG] Retrieved car listing features: {carListing.Features}");
        Console.WriteLine($"[DEBUG] Retrieved car listing images count: {carListing.Images?.Count ?? 0}");

        if (carListing.Images != null)
        {
            foreach (var image in carListing.Images)
            {
                Console.WriteLine($"[DEBUG] Image: Id={image.Id}, Url={image.ImageUrl}, IsPrimary={image.IsPrimary}");
            }
        }

        return _mapper.Map<CarListingDto>(carListing);
    }
}
