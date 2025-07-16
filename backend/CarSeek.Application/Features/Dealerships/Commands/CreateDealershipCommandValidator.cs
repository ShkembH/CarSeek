using FluentValidation;

namespace CarSeek.Application.Features.Dealerships.Commands;

public class CreateDealershipCommandValidator : AbstractValidator<CreateDealershipCommand>
{
    public CreateDealershipCommandValidator()
    {
        RuleFor(v => v.Request.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(v => v.Request.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(v => v.Request.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

        RuleFor(v => v.Request.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(v => v.Request.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");

        RuleFor(v => v.Request.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

        RuleFor(v => v.Request.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(v => v.Request.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(v => v.Request.Website)
            .MaximumLength(200).WithMessage("Website must not exceed 200 characters");
    }
}
