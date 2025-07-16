using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using CarSeek.Application.Common.Exceptions;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Entities;

namespace CarSeek.Application.Features.CarListings.Commands;

public record UploadCarImagesCommand(Guid CarListingId, UploadCarImagesRequest ImagesRequest) : IRequest<List<CarImageDto>>;

public class UploadCarImagesCommandHandler : IRequestHandler<UploadCarImagesCommand, List<CarImageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UploadCarImagesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<List<CarImageDto>> Handle(UploadCarImagesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        Console.WriteLine($"[DEBUG] Uploading images for car listing: {request.CarListingId}");
        Console.WriteLine($"[DEBUG] Number of images to upload: {request.ImagesRequest.Images.Count}");

        // Get the car listing and verify ownership
        var carListing = await _context.CarListings
            .FirstOrDefaultAsync(c => c.Id == request.CarListingId, cancellationToken);

        if (carListing == null)
        {
            throw new NotFoundException(nameof(CarListing), request.CarListingId);
        }

        if (carListing.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to upload images to this listing");
        }

        var uploadedImages = new List<CarImage>();

        foreach (var imageDto in request.ImagesRequest.Images.ToList())
        {
            Console.WriteLine($"[DEBUG] Processing image: AltText={imageDto.AltText}, IsPrimary={imageDto.IsPrimary}, DisplayOrder={imageDto.DisplayOrder}");

            // Extract the base64 data
            var base64Data = imageDto.Base64Image;
            var match = Regex.Match(base64Data, @"^data:image\/([a-zA-Z]+);base64,(.+)$");

            if (!match.Success)
            {
                throw new ValidationException("Invalid image format");
            }

            var fileExtension = match.Groups[1].Value;
            var base64Content = match.Groups[2].Value;
            var imageBytes = Convert.FromBase64String(base64Content);

            Console.WriteLine($"[DEBUG] Image details: Extension={fileExtension}, Size={imageBytes.Length} bytes");

            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}.{fileExtension}";

            // Upload the file to storage
            var imageUrl = await _fileStorageService.UploadFileAsync(
                fileName,
                imageBytes,
                $"car-listings/{request.CarListingId}",
                cancellationToken);

            Console.WriteLine($"[DEBUG] Image uploaded to: {imageUrl}");

            // Create and save the car image entity
            var carImage = new CarImage
            {
                CarListingId = request.CarListingId,
                ImageUrl = imageUrl,
                AltText = imageDto.AltText,
                DisplayOrder = imageDto.DisplayOrder,
                IsPrimary = imageDto.IsPrimary
            };

            _context.CarImages.Add(carImage);
            uploadedImages.Add(carImage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"[DEBUG] Saved {uploadedImages.Count} images to database");

        // Map to DTOs and return
        return uploadedImages.Select(img => new CarImageDto
        {
            Id = img.Id,
            ImageUrl = img.ImageUrl,
            AltText = img.AltText,
            DisplayOrder = img.DisplayOrder,
            IsPrimary = img.IsPrimary
        }).ToList();
    }
}
