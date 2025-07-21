using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Common.Models;
using CarSeek.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarSeek.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ProfileController(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> GetProfile()
    {
        if (!_currentUserService.UserId.HasValue)
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Dealership)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value);

        if (user == null)
            return NotFound();

        var response = new ProfileResponse
        {
            Email = user.Email,
            // Only set PhoneNumber once, with correct logic
            PhoneNumber = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.PhoneNumber : user.PhoneNumber,
            Country = user.Country,
            City = user.City,
            Role = user.Role.ToString(),
            FirstName = user.Role == CarSeek.Domain.Enums.UserRole.Individual ? user.FirstName : null,
            LastName = user.Role == CarSeek.Domain.Enums.UserRole.Individual ? user.LastName : null,
            CompanyName = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Name : null,
            CompanyUniqueNumber = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.CompanyUniqueNumber : null,
            BusinessCertificatePath = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.BusinessCertificatePath : null,
            Location = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Location : null,
            Description = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Description : null,
            Website = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Website : null,
            AddressStreet = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Address?.Street : null,
            AddressCity = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Address?.City : null,
            AddressState = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Address?.State : null,
            AddressPostalCode = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Address?.PostalCode : null,
            AddressCountry = user.Role == CarSeek.Domain.Enums.UserRole.Dealership ? user.Dealership?.Address?.Country : null
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        if (!_currentUserService.UserId.HasValue)
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Dealership)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value);

        if (user == null)
            return NotFound();

        // Update common fields
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Country = request.Country ?? user.Country;
        user.City = request.City ?? user.City;

        if (user.Role == CarSeek.Domain.Enums.UserRole.Individual)
        {
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
        }
        else if (user.Role == CarSeek.Domain.Enums.UserRole.Dealership && user.Dealership != null)
        {
            user.Dealership.Name = request.CompanyName ?? user.Dealership.Name;
            user.Dealership.CompanyUniqueNumber = request.CompanyUniqueNumber ?? user.Dealership.CompanyUniqueNumber;
            user.Dealership.Location = request.Location ?? user.Dealership.Location;
            // Handle business certificate upload
            if (request.BusinessCertificate != null && request.BusinessCertificate.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", "dealership-certificates");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}_{request.BusinessCertificate.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.BusinessCertificate.CopyToAsync(stream);
                }
                user.Dealership.BusinessCertificatePath = $"/uploads/dealership-certificates/{fileName}";
            }
        }

        await _context.SaveChangesAsync(default);
        return NoContent();
    }
}
