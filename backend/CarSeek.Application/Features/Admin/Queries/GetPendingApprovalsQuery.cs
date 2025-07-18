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

        var pendingDealerships = await _context.Dealerships
            .Where(d => !d.IsApproved)
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
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

        pendingApprovals.AddRange(pendingDealerships.Select(dealership => new PendingApprovalDto
        {
            Id = dealership.Id,
            Type = "Dealership",
            Title = dealership.Name,
            Submitter = $"{dealership.User.FirstName} {dealership.User.LastName}",
            SubmittedDate = dealership.CreatedAt,
            Description = dealership.Description,
            DealershipName = dealership.Name,
            DealershipDescription = dealership.Description,
            AddressStreet = dealership.Address?.Street ?? string.Empty,
            AddressCity = dealership.Address?.City ?? string.Empty,
            AddressState = dealership.Address?.State ?? string.Empty,
            AddressPostalCode = dealership.Address?.PostalCode ?? string.Empty,
            AddressCountry = dealership.Address?.Country ?? string.Empty,
            DealershipPhoneNumber = dealership.PhoneNumber,
            Website = dealership.Website,
            CompanyUniqueNumber = dealership.CompanyUniqueNumber,
            BusinessCertificatePath = dealership.BusinessCertificatePath,
            Location = dealership.Location,
            UserEmail = dealership.User.Email,
            UserFirstName = dealership.User.FirstName,
            UserLastName = dealership.User.LastName,
            UserPhoneNumber = dealership.User.PhoneNumber ?? string.Empty,
            UserCountry = dealership.User.Country ?? string.Empty,
            UserCity = dealership.User.City ?? string.Empty
        }));

        return pendingApprovals;
    }
}
