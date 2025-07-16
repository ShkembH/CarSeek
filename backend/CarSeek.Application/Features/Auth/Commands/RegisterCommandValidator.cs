using FluentValidation;

namespace CarSeek.Application.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private static readonly string[] KosovoAlbaniaCities = {
        // Kosovo cities
        "Pristina", "Prizren", "Peja", "Gjakova", "Gjilan", "Mitrovica", "Ferizaj", "Vushtrri", "Podujeva", "Suhareka",
        "Rahovec", "Lipjan", "Malisheva", "Kamenica", "Viti", "Decan", "Istog", "Kllokot", "Novoberde", "Obiliq",
        "Partesh", "Ranillug", "Gracanica", "Hani i Elezit", "Mamusa", "Junik", "Zvecan", "Zubin Potok", "Leposavic", "Mitrovica e Veriut",

        // Albania cities
        "Tirana", "Durres", "Vlora", "Elbasan", "Shkoder", "Fier", "Korce", "Berat", "Lushnje", "Kavaja",
        "Pogradec", "Gjirokaster", "Saranda", "Lac", "Kukes", "Lezha", "Patos", "Corovoda", "Puke", "Burrel",
        "Kruja", "Fushe-Kruja", "Mamurras", "Laç", "Rubik", "Bulqiza", "Klos", "Mat", "Dibra", "Librazhd",
        "Prrenjas", "Belsh", "Gramsh", "Cërrik", "Peqin", "Roskovec", "Patos", "Ballsh", "Mallakaster", "Divjaka",
        "Lushnja", "Fier", "Libofsha", "Tepelena", "Memaliaj", "Kelcyra", "Permet", "Leskovik", "Erseka", "Maliq",
        "Bilisht", "Devoll", "Pustec", "Lin", "Finiq", "Delvina", "Konispol", "Dropull", "Himara", "Selenica",
        "Orikum", "Vau i Dejes", "Koplik", "Malesi e Madhe", "Bajram Curri", "Has", "Tropoja", "Fushe Arrez", "Rreshen"
    };

    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .When(v => v.Role == CarSeek.Domain.Enums.UserRole.Individual);

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
            .When(v => v.Role == CarSeek.Domain.Enums.UserRole.Individual);

        RuleFor(v => v.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(v => !string.IsNullOrEmpty(v.PhoneNumber))
            .WithMessage("Phone number format is invalid");

        RuleFor(v => v.Country)
            .Must(country => string.IsNullOrEmpty(country) || country == "Kosovo" || country == "Albania")
            .WithMessage("Country must be either Kosovo or Albania");

        RuleFor(v => v.City)
            .Must(city => string.IsNullOrEmpty(city) || KosovoAlbaniaCities.Contains(city))
            .WithMessage("City must be a valid city from Kosovo or Albania");
    }
}
