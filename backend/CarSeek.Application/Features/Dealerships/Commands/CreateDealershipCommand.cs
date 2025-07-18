using AutoMapper;
using MediatR;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;
using CarSeek.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CarSeek.Application.Features.Dealerships.Commands;

public record CreateDealershipCommand(CreateDealershipRequest Request) : IRequest<DealershipDto>;

public class CreateDealershipCommandHandler : IRequestHandler<CreateDealershipCommand, DealershipDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateDealershipCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<DealershipDto> Handle(CreateDealershipCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(_currentUserService.UserId)
            ?? throw new NotFoundException(nameof(User), _currentUserService.UserId!);

        if (user.Role != UserRole.Dealership)
        {
            throw new ForbiddenAccessException("Only dealerships can create dealerships");
        }

        // Handle file upload
        string certificatePath = string.Empty;
        if (request.Request.BusinessCertificate != null && request.Request.BusinessCertificate.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dealership-certificates");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            var fileExt = Path.GetExtension(request.Request.BusinessCertificate.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Request.BusinessCertificate.CopyToAsync(stream, cancellationToken);
            }
            // Store relative path for later retrieval
            certificatePath = $"uploads/dealership-certificates/{fileName}";
        }

        var dealership = new Dealership
        {
            Name = request.Request.Name,
            Description = request.Request.Description,
            Address = new Address
            {
                Street = request.Request.Street,
                City = request.Request.City,
                State = request.Request.State,
                PostalCode = request.Request.PostalCode,
                Country = request.Request.Country
            },
            PhoneNumber = request.Request.PhoneNumber,
            Website = request.Request.Website,
            UserId = user.Id,
            CompanyUniqueNumber = request.Request.CompanyUniqueNumber,
            BusinessCertificatePath = certificatePath,
            Location = request.Request.Location,
            IsApproved = false // New dealership registrations are not approved by default
        };

        _context.Dealerships.Add(dealership);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DealershipDto>(dealership);
    }
}
