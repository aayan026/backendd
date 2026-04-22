using FluentValidation;
using FurnitureShop.Application.Dtos.Collection;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateCollectionValidator : AbstractValidator<CreateCollectionDto>
{
    private static readonly string[] RequiredLangs = { "az", "ru", "en" };

    public CreateCollectionValidator()
    {
        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("GreaterThanZero|TotalPrice");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0).When(x => x.DiscountPrice.HasValue)
            .WithMessage("GreaterThanZero|DiscountPrice")
            .LessThan(x => x.TotalPrice).When(x => x.DiscountPrice.HasValue)
            .WithMessage("DiscountLessThanTotal|DiscountPrice");

        RuleFor(x => x.CollectionCategoryId)
            .GreaterThan(0).WithMessage("InvalidId|CategoryId");

        RuleFor(x => x.ProductIds)
            .NotNull().WithMessage("Required|Products")
            .Must(x => x.Count > 0).WithMessage("MinCount|Products|1");

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
