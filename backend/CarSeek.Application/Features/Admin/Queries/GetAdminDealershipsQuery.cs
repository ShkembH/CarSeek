using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Dealerships.DTOs;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetAdminDealershipsQuery : IRequest<List<DealershipDto>>;

public class GetAdminDealershipsQueryHandler : IRequestHandler<GetAdminDealershipsQuery, List<DealershipDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAdminDealershipsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DealershipDto>> Handle(GetAdminDealershipsQuery request, CancellationToken cancellationToken)
    {
        var dealerships = await _context.Dealerships
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<DealershipDto>>(dealerships);
    }
}
