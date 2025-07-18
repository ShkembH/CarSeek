using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.Admin.Queries;
using CarSeek.Application.Features.Admin.DTOs;
using CarSeek.Application.Features.CarListings.Commands;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Common.Interfaces;

namespace CarSeek.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public AdminController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats()
    {
        return await Mediator.Send(new GetAdminStatsQuery());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        return await Mediator.Send(new GetUsersQuery());
    }

    [HttpGet("listings")]
    public async Task<ActionResult<List<CarListingDto>>> GetListings()
    {
        return await Mediator.Send(new GetAdminListingsQuery());
    }

    [HttpGet("dealerships")]
    public async Task<ActionResult<List<DealershipDto>>> GetDealerships()
    {
        return await Mediator.Send(new GetAdminDealershipsQuery());
    }

    [HttpGet("pending-approvals")]
    public async Task<ActionResult<List<PendingApprovalDto>>> GetPendingApprovals()
    {
        return await Mediator.Send(new GetPendingApprovalsQuery());
    }

    [HttpGet("recent-activity")]
    public async Task<ActionResult<List<ActivityLogDto>>> GetRecentActivity()
    {
        return await Mediator.Send(new GetRecentActivityQuery());
    }

    [HttpPatch("listings/{id}/approve")]
    public async Task<ActionResult> ApproveListing(Guid id)
    {
        await Mediator.Send(new UpdateListingStatusCommand(id, new UpdateListingStatusRequest { Status = ListingStatus.Active }));
        return Ok();
    }

    [HttpPatch("dealerships/{id}/approve")]
    public async Task<ActionResult> ApproveDealership(Guid id)
    {
        var dealership = _context.Dealerships.FirstOrDefault(d => d.Id == id);
        if (dealership == null)
            return NotFound();
        
        dealership.IsApproved = true;
        
        // Update the user status to approved and active
        var user = _context.Users.FirstOrDefault(u => u.Id == dealership.UserId);
        if (user != null)
        {
            user.Status = UserStatus.Approved;
            user.IsActive = true;
        }
        
        await _context.SaveChangesAsync(default);
        return Ok();
    }

    [HttpPatch("dealerships/{id}/reject")]
    public async Task<ActionResult> RejectDealership(Guid id)
    {
        var dealership = _context.Dealerships.FirstOrDefault(d => d.Id == id);
        if (dealership == null)
            return NotFound();
        
        // Update the user status to rejected and inactive
        var user = _context.Users.FirstOrDefault(u => u.Id == dealership.UserId);
        if (user != null)
        {
            user.Status = UserStatus.Rejected;
            user.IsActive = false;
        }
        
        // Remove the dealership record
        _context.Dealerships.Remove(dealership);
        await _context.SaveChangesAsync(default);
        return Ok();
    }

    [HttpPatch("listings/{id}/reject")]
    public async Task<ActionResult> RejectListing(Guid id)
    {
        await Mediator.Send(new UpdateListingStatusCommand(id, new UpdateListingStatusRequest { Status = ListingStatus.Rejected }));
        return Ok();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Delete all car listings for this user
        var listings = _context.CarListings.Where(l => l.UserId == id);
        _context.CarListings.RemoveRange(listings);

        // Delete dealership profile if exists
        var dealership = _context.Dealerships.FirstOrDefault(d => d.UserId == id);
        if (dealership != null)
        {
            _context.Dealerships.Remove(dealership);
        }

        // Delete all saved listings for this user
        var savedListings = _context.SavedListings.Where(sl => sl.UserId == id);
        _context.SavedListings.RemoveRange(savedListings);

        // Delete all activity logs for this user
        var activityLogs = _context.ActivityLogs.Where(a => a.UserId == id);
        _context.ActivityLogs.RemoveRange(activityLogs);

        // Delete all chat messages for this user (as sender or recipient)
        var chatMessages = _context.ChatMessages.Where(m => m.SenderId == id || m.RecipientId == id);
        _context.ChatMessages.RemoveRange(chatMessages);

        // Hard delete: remove user from database
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(default);
        return NoContent();
    }
}
