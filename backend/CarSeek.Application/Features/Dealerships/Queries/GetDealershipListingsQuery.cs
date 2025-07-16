using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Common.Exceptions;

namespace CarSeek.Application.Features.Dealerships.Queries;

public record GetDealershipListingsQuery(
    Guid DealershipId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedCarListingsResponse>;

public class GetDealershipListingsQueryHandler : IRequestHandler<GetDealershipListingsQuery, PaginatedCarListingsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDealershipListingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedCarListingsResponse> Handle(GetDealershipListingsQuery request, CancellationToken cancellationToken)
    {
        // Verify dealership exists
        var dealershipExists = await _context.Dealerships
            .AnyAsync(d => d.Id == request.DealershipId, cancellationToken);

        if (!dealershipExists)
        {
            throw new NotFoundException("Dealership", request.DealershipId);
        }

        var query = _context.CarListings
            .Include(c => c.Dealership)
                .ThenInclude(d => d.User)
            .Include(c => c.User)
            .Where(c => c.DealershipId == request.DealershipId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var carListings = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedCarListingsResponse
        {
            Items = _mapper.Map<IEnumerable<CarListingDto>>(carListings),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            TotalPages = totalPages
        };
    }
}
