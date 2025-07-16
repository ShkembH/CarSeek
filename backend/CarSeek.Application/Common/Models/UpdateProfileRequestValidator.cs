using FluentValidation;
using Microsoft.AspNetCore.Http;
using CarSeek.Domain.Enums;

namespace CarSeek.Application.Common.Models;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number format is invalid");

        // Individual validation
        When(x => x.FirstName != null || x.LastName != null, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required for individuals.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required for individuals.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");
        });

        // Dealership validation
        When(x => x.CompanyName != null || x.CompanyUniqueNumber != null || x.Location != null || x.BusinessCertificate != null, () =>
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required for dealerships.")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters.");
            RuleFor(x => x.CompanyUniqueNumber)
                .NotEmpty().WithMessage("Company unique number is required for dealerships.")
                .MaximumLength(100).WithMessage("Company unique number must not exceed 100 characters.");
            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required for dealerships.")
                .MaximumLength(100).WithMessage("Location must not exceed 100 characters.");
            RuleFor(x => x.BusinessCertificate)
                .Must(BeAValidFileType)
                .When(x => x.BusinessCertificate != null)
                .WithMessage("Business certificate must be a PDF or image file.");
        });
    }

    private bool BeAValidFileType(IFormFile? file)
    {
        if (file == null) return true;
        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
        return allowedTypes.Contains(file.ContentType);
    }
}
