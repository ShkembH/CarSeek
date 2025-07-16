using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Admin.DTOs;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetRecentActivityQuery : IRequest<List<ActivityLogDto>>;

public class GetRecentActivityQueryHandler : IRequestHandler<GetRecentActivityQuery, List<ActivityLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentActivityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityLogDto>> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
    {
        return await _context.ActivityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                Message = a.Message,
                Type = a.Type,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "System",
                CreatedAt = a.CreatedAt,
                EntityId = a.EntityId,
                EntityType = a.EntityType
            })
            .ToListAsync(cancellationToken);
    }
}
