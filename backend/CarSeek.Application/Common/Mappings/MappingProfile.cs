
using AutoMapper;
using CarSeek.Domain.Entities;
using CarSeek.Application.Features.Auth.DTOs;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Features.Admin.DTOs;

namespace CarSeek.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, AuthResponse>()
            .ForMember(d => d.Token, opt => opt.Ignore());

        // Add User to UserDto mapping
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<CarImage, CarImageDto>();
        CreateMap<CarListing, CarListingDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(src =>
                src.Dealership != null ? src.Dealership.Name : $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(d => d.OwnerEmail, opt => opt.MapFrom(src =>
                src.Dealership != null ? src.Dealership.User.Email : src.User.Email))
            .ForMember(d => d.OwnerPhone, opt => opt.MapFrom(src =>
                src.Dealership != null ? null : src.User.PhoneNumber))
            .ForMember(d => d.PrimaryImageUrl, opt => opt.MapFrom<PrimaryImageUrlResolver>())
            .ForMember(d => d.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<CreateCarListingRequest, CarListing>();

        CreateMap<Dealership, DealershipDto>();
    }
}
