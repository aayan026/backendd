using FluentValidation;
using FurnitureShop.Application.Dtos.Product;

namespace FurnitureShop.Application.Validation.Concrete;

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    private static readonly string[] RequiredLangs = { "az", "ru", "en" };

    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("InvalidId|Id");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("GreaterThanZero|Price");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("GreaterThanZero|Stock");

        RuleFor(x => x.FurnitureCategoryId)
            .GreaterThan(0).WithMessage("InvalidId|CategoryId");

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
            t.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Required|Name")
                .MaximumLength(200).WithMessage("MaxLength|Name|200");
        });
    }
}
