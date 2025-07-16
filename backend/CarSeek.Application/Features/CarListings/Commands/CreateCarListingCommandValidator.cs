using FluentValidation;

namespace CarSeek.Application.Features.CarListings.Commands;

public class CreateCarListingCommandValidator : AbstractValidator<CreateCarListingCommand>
{
    public CreateCarListingCommandValidator()
    {
        RuleFor(v => v.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(v => v.Request.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(v => v.Request.Year)
            .NotEmpty().WithMessage("Year is required")
            .GreaterThan(1900).WithMessage("Year must be greater than 1900")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage("Year cannot be in the future");

        RuleFor(v => v.Request.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(50).WithMessage("Make must not exceed 50 characters");

        RuleFor(v => v.Request.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(50).WithMessage("Model must not exceed 50 characters");

        RuleFor(v => v.Request.Price)
            .NotEmpty().WithMessage("Price is required")
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(v => v.Request.Mileage)
            .NotEmpty().WithMessage("Mileage is required")
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must be greater than or equal to 0");
    }
}
