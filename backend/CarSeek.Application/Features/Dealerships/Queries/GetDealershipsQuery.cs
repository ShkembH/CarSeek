using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Dealerships.DTOs;

namespace CarSeek.Application.Features.Dealerships.Queries;

public record GetDealershipsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PaginatedDealershipsResponse>;

public class GetDealershipsQueryHandler : IRequestHandler<GetDealershipsQuery, PaginatedDealershipsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDealershipsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedDealershipsResponse> Handle(GetDealershipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Dealerships.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(d =>
                d.Name.Contains(request.SearchTerm) ||
                d.Description.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var dealerships = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedDealershipsResponse
        {
            Items = _mapper.Map<IEnumerable<DealershipDto>>(dealerships),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            TotalPages = totalPages
        };
    }
}
