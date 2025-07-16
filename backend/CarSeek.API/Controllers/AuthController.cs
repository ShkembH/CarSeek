using MediatR;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Features.Auth.Commands;
using CarSeek.Application.Features.Auth.Common;

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
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Country,
            request.City,
            request.Role,
            request.CompanyName,
            request.CompanyUniqueNumber,
            request.Location,
            request.BusinessCertificatePath
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
