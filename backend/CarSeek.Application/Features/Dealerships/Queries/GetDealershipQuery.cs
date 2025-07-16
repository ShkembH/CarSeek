using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Dealerships.DTOs;

namespace CarSeek.Application.Features.Dealerships.Queries;

public record GetDealershipQuery(Guid Id) : IRequest<DealershipDto>;

public class GetDealershipQueryHandler : IRequestHandler<GetDealershipQuery, DealershipDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDealershipQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DealershipDto> Handle(GetDealershipQuery request, CancellationToken cancellationToken)
    {
        var dealership = await _context.Dealerships
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (dealership == null)
        {
            throw new NotFoundException(nameof(dealership), request.Id);
        }

        return _mapper.Map<DealershipDto>(dealership);
    }
}
