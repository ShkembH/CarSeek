using FluentValidation;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Features.CarListings.Commands;

public class UpdateListingStatusCommandValidator : AbstractValidator<UpdateListingStatusCommand>
{
    public UpdateListingStatusCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Request.Status)
            .IsInEnum()
            .NotEqual(ListingStatus.Pending)
            .WithMessage("Cannot manually set status to Pending");
    }
}
