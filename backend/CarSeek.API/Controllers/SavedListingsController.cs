using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Features.Dealerships.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;

namespace CarSeek.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SavedListingsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SavedListingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/SavedListings/{listingId}
    [HttpPost("{listingId}")]
    public async Task<IActionResult> SaveListing(Guid listingId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var exists = await _context.SavedListings.AnyAsync(sl => sl.UserId == userId && sl.CarListingId == listingId);
            if (exists) return BadRequest("Listing already saved.");

            var saved = new SavedListing
            {
                UserId = userId.Value,
                CarListingId = listingId,
                CreatedAt = DateTime.UtcNow
            };
            _context.SavedListings.Add(saved);
            await _context.SaveChangesAsync(default);
            return Ok(new { success = true, message = "Listing saved successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveListing: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // DELETE: api/SavedListings/{listingId}
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> UnsaveListing(Guid listingId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var saved = await _context.SavedListings.FirstOrDefaultAsync(sl => sl.UserId == userId && sl.CarListingId == listingId);
            if (saved == null) return NotFound();

            _context.SavedListings.Remove(saved);
            await _context.SaveChangesAsync(default);
            return Ok(new { success = true, message = "Listing unsaved successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UnsaveListing: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: api/SavedListings
    [HttpGet]
    public async Task<ActionResult<List<CarListingDto>>> GetSavedListings()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Simplified query to debug the issue
            var savedListings = await _context.SavedListings
                .Where(sl => sl.UserId == userId)
                .ToListAsync();

            var listingIds = savedListings.Select(sl => sl.CarListingId).ToList();

            var listings = await _context.CarListings
                .Where(cl => listingIds.Contains(cl.Id))
                .Include(cl => cl.Images)
                .Include(cl => cl.User)
                .Include(cl => cl.Dealership)
                .ToListAsync();

            // Map to DTOs to avoid circular references
            var listingDtos = listings.Select(listing => new CarListingDto
            {
                Id = listing.Id,
                Title = listing.Title,
                Description = listing.Description,
                Year = listing.Year,
                Make = listing.Make,
                Model = listing.Model,
                Price = listing.Price,
                Mileage = listing.Mileage,
                Status = listing.Status,
                FuelType = listing.FuelType,
                Transmission = listing.Transmission,
                Color = listing.Color,
                Images = listing.Images?.Select(img => new CarImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    AltText = img.AltText,
                    DisplayOrder = img.DisplayOrder,
                    IsPrimary = img.IsPrimary
                }).ToList() ?? new List<CarImageDto>(),
                OwnerName = listing.User != null ? $"{listing.User.FirstName} {listing.User.LastName}" : "Unknown",
                OwnerEmail = listing.User?.Email ?? "",
                Dealership = listing.Dealership != null ? new DealershipDto
                {
                    Id = listing.Dealership.Id,
                    Name = listing.Dealership.Name,
                    Address = listing.Dealership.Address
                } : null,
                PrimaryImageUrl = listing.Images?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl ??
                                 listing.Images?.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl
            }).ToList();

            return Ok(listingDtos);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.WriteLine($"Error in GetSavedListings: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private Guid? GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }
}
