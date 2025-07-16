using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Common.Exceptions;

namespace CarSeek.Application.Features.CarListings.Queries;

public record GetUserCarListingsQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedCarListingsResponse>;

public class GetUserCarListingsQueryHandler : IRequestHandler<GetUserCarListingsQuery, PaginatedCarListingsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetUserCarListingsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedCarListingsResponse> Handle(GetUserCarListingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User must be authenticated to view their listings");
        }

        var query = _context.CarListings
            .Include(c => c.Dealership)
                .ThenInclude(d => d.User)
            .Include(c => c.User)
            .Where(c => c.UserId == userId.Value || c.Dealership.UserId == userId.Value)
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
