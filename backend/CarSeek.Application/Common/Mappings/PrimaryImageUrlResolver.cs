using AutoMapper;
using CarSeek.Domain.Entities;
using CarSeek.Application.Features.CarListings.DTOs;
using System.Linq;

namespace CarSeek.Application.Common.Mappings
{
    public class PrimaryImageUrlResolver : IValueResolver<CarListing, CarListingDto, string>
    {
        public string Resolve(CarListing src, CarListingDto dest, string destMember, ResolutionContext context)
        {
            var primary = src.Images.FirstOrDefault(i => i.IsPrimary);
            if (primary != null)
                return primary.ImageUrl;
            var first = src.Images.FirstOrDefault();
            return first != null ? first.ImageUrl : null;
        }
    }
}
