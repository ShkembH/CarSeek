using System.Collections.Generic;

namespace CarSeek.Application.Features.CarListings.DTOs;

public class UploadCarImagesRequest
{
    public List<CarImageUploadDto> Images { get; set; } = new();
}

public class CarImageUploadDto
{
    public string Base64Image { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
