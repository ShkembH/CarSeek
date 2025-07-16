using FluentValidation;

namespace CarSeek.Application.Features.CarListings.Commands;

public class DeleteCarListingCommandValidator : AbstractValidator<DeleteCarListingCommand>
{
    public DeleteCarListingCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
