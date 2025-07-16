using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Admin.DTOs;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetAdminStatsQuery : IRequest<AdminStatsResponse>;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetAdminStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsResponse> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalListings = await _context.CarListings.CountAsync(cancellationToken);
        var totalDealerships = await _context.Dealerships.CountAsync(cancellationToken);
        var pendingApprovals = await _context.CarListings
            .CountAsync(x => x.Status == ListingStatus.Pending, cancellationToken);
        var activeUsers = await _context.Users
            .CountAsync(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-30), cancellationToken);

        return new AdminStatsResponse
        {
            TotalUsers = totalUsers,
            TotalListings = totalListings,
            TotalDealerships = totalDealerships,
            PendingApprovals = pendingApprovals,
            MonthlyRevenue = 0, // Implement based on your business logic
            ActiveUsers = activeUsers
        };
    }
}
