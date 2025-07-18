using MediatR;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.Auth.Commands;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Domain.Enums;

namespace CarSeek.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromForm] IFormCollection form)
    {
        // Parse form data manually
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var firstName = form["firstName"].ToString();
        var lastName = form["lastName"].ToString();
        var phoneNumber = form["phoneNumber"].ToString();
        var country = form["country"].ToString();
        var city = form["city"].ToString();
        var role = Enum.Parse<UserRole>(form["role"].ToString());
        
        string? companyName = null;
        string? companyUniqueNumber = null;
        string? location = null;
        IFormFile? businessCertificate = null;
        
        if (role == UserRole.Dealership)
        {
            companyName = form["companyName"].ToString();
            companyUniqueNumber = form["companyUniqueNumber"].ToString();
            location = form["location"].ToString();
            businessCertificate = form.Files.FirstOrDefault(f => f.Name == "businessCertificate");
        }

        var command = new RegisterCommand(
            email,
            password,
            firstName,
            lastName,
            phoneNumber,
            country,
            city,
            role,
            companyName,
            companyUniqueNumber,
            location,
            businessCertificate
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password);

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
