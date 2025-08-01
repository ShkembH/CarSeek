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
    private readonly ICacheService _cacheService;

    public GetCarListingsQueryHandler(IApplicationDbContext context, IMapper mapper, ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PaginatedCarListingsResponse> Handle(GetCarListingsQuery request, CancellationToken cancellationToken)
    {
        // Generate cache key based on query parameters
        var cacheKey = GenerateCacheKey(request);
        
        // Try to get from cache first
        var cachedResult = await _cacheService.GetAsync<PaginatedCarListingsResponse>(cacheKey);
        if (cachedResult != null)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedResult;
        }

        Console.WriteLine($"Cache miss for key: {cacheKey}, querying database...");

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
        }

        var result = new PaginatedCarListingsResponse
        {
            Items = _mapper.Map<List<CarListingDto>>(paginatedListings),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            TotalPages = totalPages
        };

        // Cache the result for 5 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    private string GenerateCacheKey(GetCarListingsQuery request)
    {
        var parameters = new[]
        {
            request.PageNumber.ToString(),
            request.PageSize.ToString(),
            request.SearchTerm ?? "",
            request.Make ?? "",
            request.Model ?? "",
            request.MinYear?.ToString() ?? "",
            request.MaxYear?.ToString() ?? "",
            request.MinPrice?.ToString() ?? "",
            request.MaxPrice?.ToString() ?? ""
        };

        return $"car_listings:{string.Join(":", parameters)}";
    }
}
