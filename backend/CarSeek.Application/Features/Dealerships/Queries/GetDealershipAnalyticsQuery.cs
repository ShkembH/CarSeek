using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Dealerships.Queries;

public record GetDealershipAnalyticsQuery(Guid DealershipId) : IRequest<DealershipAnalyticsDto>;

public class GetDealershipAnalyticsQueryHandler : IRequestHandler<GetDealershipAnalyticsQuery, DealershipAnalyticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDealershipAnalyticsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DealershipAnalyticsDto> Handle(GetDealershipAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User must be authenticated");
        }

        // Verify dealership exists and user owns it
        var dealership = await _context.Dealerships
            .FirstOrDefaultAsync(d => d.Id == request.DealershipId && d.UserId == userId.Value, cancellationToken);

        if (dealership == null)
        {
            throw new NotFoundException("Dealership", request.DealershipId);
        }

        var listings = await _context.CarListings
            .Where(c => c.DealershipId == request.DealershipId)
            .ToListAsync(cancellationToken);

        var totalListings = listings.Count;
        var activeListings = listings.Count(l => l.Status == ListingStatus.Active);
        var pendingListings = listings.Count(l => l.Status == ListingStatus.Pending);
        var soldListings = listings.Count(l => l.Status == ListingStatus.Sold);
        var totalValue = listings.Where(l => l.Status == ListingStatus.Active).Sum(l => l.Price);
        var averagePrice = activeListings > 0 ? totalValue / activeListings : 0;

        // Calculate monthly stats for the last 12 months
        var monthlyStats = new List<MonthlyStatsDto>();
        var currentDate = DateTime.UtcNow;

        for (int i = 11; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var monthListings = listings.Where(l => l.CreatedAt >= monthStart && l.CreatedAt < monthEnd);
            var monthSold = listings.Where(l => l.UpdatedAt >= monthStart && l.UpdatedAt < monthEnd && l.Status == ListingStatus.Sold);

            monthlyStats.Add(new MonthlyStatsDto
            {
                Month = targetDate.Month,
                Year = targetDate.Year,
                ListingsCreated = monthListings.Count(),
                ListingsSold = monthSold.Count(),
                Revenue = monthSold.Sum(l => l.Price)
            });
        }

        return new DealershipAnalyticsDto
        {
            TotalListings = totalListings,
            ActiveListings = activeListings,
            PendingListings = pendingListings,
            SoldListings = soldListings,
            TotalValue = totalValue,
            AveragePrice = averagePrice,
            ViewsThisMonth = 0, // This would require a separate tracking system
            InquiriesThisMonth = 0, // This would require a separate tracking system
            MonthlyStats = monthlyStats
        };
    }
}
