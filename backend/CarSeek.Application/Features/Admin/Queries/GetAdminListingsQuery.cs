using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetAdminListingsQuery : IRequest<List<CarListingDto>>;

public class GetAdminListingsQueryHandler : IRequestHandler<GetAdminListingsQuery, List<CarListingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAdminListingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CarListingDto>> Handle(GetAdminListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await _context.CarListings
            .Include(c => c.Dealership)
            .Include(c => c.User)
            .Include(c => c.Images)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CarListingDto>>(listings);
    }
}
