using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Admin.DTOs;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetUsersQuery : IRequest<List<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<UserDto>>(users);
    }
}
