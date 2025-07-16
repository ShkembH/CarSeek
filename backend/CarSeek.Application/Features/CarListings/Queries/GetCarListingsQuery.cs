using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums; // Add this missing using statement

namespace CarSeek.Application.Features.CarListings.Queries;

public record GetCarListingsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Make = null,
    string? Model = null,
    int? MinYear = null,
    int? MaxYear = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null) : IRequest<PaginatedCarListingsResponse>;

public class GetCarListingsQueryHandler : IRequestHandler<GetCarListingsQuery, PaginatedCarListingsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCarListingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedCarListingsResponse> Handle(GetCarListingsQuery request, CancellationToken cancellationToken)
    {
        // Start with a queryable and then apply filters
        var carListings = _context.CarListings
            .Include(c => c.Dealership)
            .Include(c => c.User)
            .Include(c => c.Images) // Include images
            .Where(c => c.Status == ListingStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            carListings = carListings.Where(c =>
                c.Title.Contains(request.SearchTerm) ||
                c.Description.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Make))
        {
            carListings = carListings.Where(c => c.Make == request.Make);
        }

        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            carListings = carListings.Where(c => c.Model == request.Model);
        }

        if (request.MinYear.HasValue)
        {
            carListings = carListings.Where(c => c.Year >= request.MinYear.Value);
        }

        if (request.MaxYear.HasValue)
        {
            carListings = carListings.Where(c => c.Year <= request.MaxYear.Value);
        }

        if (request.MinPrice.HasValue)
        {
            carListings = carListings.Where(c => c.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            carListings = carListings.Where(c => c.Price <= request.MaxPrice.Value);
        }

        var totalCount = await carListings.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var paginatedListings = await carListings
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Add logging
        Console.WriteLine($"Found {paginatedListings.Count} active listings out of total {totalCount}");
        foreach (var listing in paginatedListings)
        {
            Console.WriteLine($"Listing: {listing.Id} - {listing.Title} - Status: {listing.Status}");
            Console.WriteLine($"  Images count: {listing.Images?.Count ?? 0}");
            if (listing.Images != null)
            {
                foreach (var image in listing.Images)
                {
                    Console.WriteLine($"    Image: {image.ImageUrl} (Primary: {image.IsPrimary})");
                }
            }
        }

        return new PaginatedCarListingsResponse
        {
            Items = _mapper.Map<IEnumerable<CarListingDto>>(paginatedListings),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            TotalPages = totalPages
        };
    }
}
