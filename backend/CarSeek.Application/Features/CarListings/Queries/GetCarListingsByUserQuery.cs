using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;

namespace CarSeek.Application.Features.CarListings.Queries;

public record GetCarListingsByUserQuery(Guid UserId) : IRequest<List<CarListingDto>>;

public class GetCarListingsByUserQueryHandler : IRequestHandler<GetCarListingsByUserQuery, List<CarListingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetCarListingsByUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<List<CarListingDto>> Handle(GetCarListingsByUserQuery request, CancellationToken cancellationToken)
    {
        var listings = await _context.CarListings
            .Where(l => l.UserId == request.UserId)
            .Include(l => l.Images)
            .Include(l => l.User)
            .Include(l => l.Dealership)
            .ToListAsync(cancellationToken);
        return listings.Select(l => _mapper.Map<CarListingDto>(l)).ToList();
    }
}
