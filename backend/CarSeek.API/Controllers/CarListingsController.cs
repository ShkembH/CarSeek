using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.CarListings.Commands;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Features.CarListings.Queries;

namespace CarSeek.API.Controllers;

public class CarListingsController : ApiControllerBase
{
    // Sellers need auth to create listings
    [HttpPost]
    [Authorize] // Keep this for creating listings
    public async Task<ActionResult<CarListingDto>> Create(CreateCarListingRequest request)
    {
        return await Mediator.Send(new CreateCarListingCommand(request));
    }

    // Buyers don't need auth to view individual cars
    [HttpGet("{id}")]
    [AllowAnonymous] // Already correct
    public async Task<ActionResult<CarListingDto>> GetById(Guid id)
    {
        return await Mediator.Send(new GetCarListingQuery(id));
    }

    // Buyers don't need auth to browse cars
    [HttpGet]
    [AllowAnonymous] // Already correct
    public async Task<ActionResult<PaginatedCarListingsResponse>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? make = null,
        [FromQuery] string? model = null,
        [FromQuery] int? minYear = null,
        [FromQuery] int? maxYear = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        return await Mediator.Send(new GetCarListingsQuery(
            pageNumber, pageSize, searchTerm, make, model,
            minYear, maxYear, minPrice, maxPrice));
    }

    // Only listing owner can update
    [HttpPut("{id}")]
    [Authorize] // Sellers need auth to update their listings
    public async Task<ActionResult<CarListingDto>> Update(Guid id, UpdateCarListingRequest request)
    {
        return await Mediator.Send(new UpdateCarListingCommand(id, request));
    }

    // Only listing owner can delete
    [HttpDelete("{id}")]
    [Authorize] // Sellers need auth to delete their listings
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCarListingCommand(id));
        return NoContent();
    }

    // Only dealerships can update status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Dealership")] // Already correct
    public async Task<ActionResult<CarListingDto>> UpdateStatus(Guid id, UpdateListingStatusRequest request)
    {
        return await Mediator.Send(new UpdateListingStatusCommand(id, request));
    }

    // Remove this endpoint until GetUserCarListingsQuery is implemented
    // Uncomment this endpoint
    [HttpGet("my-listings")]
    [Authorize]
    public async Task<ActionResult<PaginatedCarListingsResponse>> GetMyListings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return await Mediator.Send(new GetUserCarListingsQuery(pageNumber, pageSize));
    }

    // Add this new endpoint
    [HttpPost("{id}/images")]
    [Authorize]
    public async Task<ActionResult<List<CarImageDto>>> UploadImages(Guid id, UploadCarImagesRequest request)
    {
        return await Mediator.Send(new UploadCarImagesCommand(id, request));
    }

    // Get all listings for a specific user
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CarListingDto>>> GetListingsByUser(Guid userId)
    {
        var listings = await Mediator.Send(new GetCarListingsByUserQuery(userId));
        return Ok(listings);
    }

    // Delete all listings for a specific user
    [HttpDelete("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteListingsByUser(Guid userId)
    {
        await Mediator.Send(new DeleteCarListingsByUserCommand(userId));
        return NoContent();
    }
}
