using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.Dealerships.Commands;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Features.Dealerships.Queries;

namespace CarSeek.API.Controllers;

public class DealershipsController : ApiControllerBase
{
    [HttpPost]
    [Authorize] // Add authorization for creating dealerships
    public async Task<ActionResult<DealershipDto>> Create([FromForm] CreateDealershipRequest request)
    {
        return await Mediator.Send(new CreateDealershipCommand(request));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<DealershipDto>> GetById(Guid id)
    {
        return await Mediator.Send(new GetDealershipQuery(id));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedDealershipsResponse>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        return await Mediator.Send(new GetDealershipsQuery(pageNumber, pageSize, searchTerm));
    }

    // Remove these endpoints until the queries and DTOs are implemented
    // [HttpGet("{id}/listings")]
    // [AllowAnonymous]
    // public async Task<ActionResult<PaginatedCarListingsResponse>> GetDealershipListings(
    //     Guid id,
    //     [FromQuery] int pageNumber = 1,
    //     [FromQuery] int pageSize = 10)
    // {
    //     return await Mediator.Send(new GetDealershipListingsQuery(id, pageNumber, pageSize));
    // }

    // [HttpGet("{id}/analytics")]
    // [Authorize(Roles = "Dealership")]
    // public async Task<ActionResult<DealershipAnalyticsDto>> GetAnalytics(Guid id)
    // {
    //     return await Mediator.Send(new GetDealershipAnalyticsQuery(id));
    // }
}
