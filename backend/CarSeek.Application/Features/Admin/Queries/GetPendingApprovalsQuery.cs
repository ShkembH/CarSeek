using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Admin.DTOs;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.Admin.Queries;

public record GetPendingApprovalsQuery : IRequest<List<PendingApprovalDto>>;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, List<PendingApprovalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPendingApprovalsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PendingApprovalDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingListings = await _context.CarListings
            .Where(x => x.Status == ListingStatus.Pending)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var pendingApprovals = pendingListings.Select(listing => new PendingApprovalDto
        {
            Id = listing.Id,
            Type = "Listing",
            Title = $"{listing.Year} {listing.Make} {listing.Model}",
            Submitter = $"{listing.User.FirstName} {listing.User.LastName}",
            SubmittedDate = listing.CreatedAt,
            Description = listing.Description
        }).ToList();

        return pendingApprovals;
    }
}
