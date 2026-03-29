using FluentValidation;
using FurnitureShop.Application.Dtos.HeroSection;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateHeroSectionValidator : AbstractValidator<CreateHeroSectionDto>
{
    private static readonly string[] RequiredLangs = { "az", "ru", "en" };

    public CreateHeroSectionValidator()
    {
        // 3 dil məcburi
        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Required|Translations")
            .Must(t => t != null && RequiredLangs.All(lang => t.Any(x => x.Lang == lang)))
            .WithMessage("Az, ru, en dillərinin hamısı üçün tərcümə tələb olunur");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.Lang)
                .NotEmpty().WithMessage("Required|LangCode")
                .Must(l => RequiredLangs.Contains(l)).WithMessage("InvalidLang|LangCode");
            t.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Required|Name")
                .MaximumLength(200).WithMessage("MaxLength|Name|200");
        });
    }
}
