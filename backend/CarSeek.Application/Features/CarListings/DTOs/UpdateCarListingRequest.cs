namespace CarSeek.Application.Features.CarListings.DTOs;

public class UpdateCarListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Mileage { get; set; }
}
