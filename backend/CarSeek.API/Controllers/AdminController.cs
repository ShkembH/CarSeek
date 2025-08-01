using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.Admin.Queries;
using CarSeek.Application.Features.Admin.DTOs;
using CarSeek.Application.Features.CarListings.Commands;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarSeek.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var user = await _context.Users.Include(u => u.Dealership).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound();
        var dto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Country = user.Country,
            City = user.City
        };
        if (user.Role == CarSeek.Domain.Enums.UserRole.Dealership && user.Dealership != null)
        {
            dto.CompanyName = user.Dealership.Name;
            dto.CompanyUniqueNumber = user.Dealership.CompanyUniqueNumber;
            dto.Location = user.Dealership.Location;
            dto.DealershipPhoneNumber = user.Dealership.PhoneNumber;
            dto.Website = user.Dealership.Website;
            dto.BusinessCertificatePath = user.Dealership.BusinessCertificatePath;
            dto.Description = user.Dealership.Description;
            dto.AddressStreet = user.Dealership.Address?.Street;
            dto.AddressCity = user.Dealership.Address?.City;
            dto.AddressState = user.Dealership.Address?.State;
            dto.AddressPostalCode = user.Dealership.Address?.PostalCode;
            dto.AddressCountry = user.Dealership.Address?.Country;
            dto.IsDealershipApproved = user.Dealership.IsApproved;
        }
        return Ok(dto);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.Include(u => u.Dealership).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound();

        // Debug: log incoming request
        Console.WriteLine($"[ADMIN UPDATE] Incoming request for user {id}: {System.Text.Json.JsonSerializer.Serialize(request)}");

        // Update common fields
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Country = request.Country ?? user.Country;
        user.City = request.City ?? user.City;
        user.IsActive = request.IsActive ?? user.IsActive;

        // Debug: log updated fields
        Console.WriteLine($"[ADMIN UPDATE] Updated user fields: Phone={user.PhoneNumber}, Country={user.Country}, City={user.City}, IsActive={user.IsActive}");

        if (user.Role == CarSeek.Domain.Enums.UserRole.Individual)
        {
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            Console.WriteLine($"[ADMIN UPDATE] Individual: FirstName={user.FirstName}, LastName={user.LastName}");
        }
        else if (user.Role == CarSeek.Domain.Enums.UserRole.Dealership && user.Dealership != null)
        {
            user.Dealership.Name = request.CompanyName ?? user.Dealership.Name;
            user.Dealership.CompanyUniqueNumber = request.CompanyUniqueNumber ?? user.Dealership.CompanyUniqueNumber;
            user.Dealership.Location = request.Location ?? user.Dealership.Location;
            user.Dealership.PhoneNumber = request.DealershipPhoneNumber ?? user.Dealership.PhoneNumber;
            user.Dealership.Website = request.Website ?? user.Dealership.Website;
            user.Dealership.Description = request.Description ?? user.Dealership.Description;
            Console.WriteLine($"[ADMIN UPDATE] Dealership: Name={user.Dealership.Name}, UniqueNumber={user.Dealership.CompanyUniqueNumber}, Location={user.Dealership.Location}, Phone={user.Dealership.PhoneNumber}, Website={user.Dealership.Website}, Description={user.Dealership.Description}");
        }

        await _context.SaveChangesAsync(default);
        Console.WriteLine($"[ADMIN UPDATE] SaveChangesAsync called for user {id}");
        return Ok();
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
