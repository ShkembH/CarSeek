using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.CarListings.DTOs;

public class UpdateListingStatusRequest
{
    public ListingStatus Status { get; set; }
}
