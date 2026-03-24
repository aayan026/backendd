using FluentValidation;
using FurnitureShop.Application.Dtos.CollectionCategory;

namespace FurnitureShop.Application.Validation.Concrete;

public class CollectionCategoryTranslationValidator : AbstractValidator<CollectionCategoryTranslationDto>
{
    public CollectionCategoryTranslationValidator()
    {
        RuleFor(x => x.Lang)
            .NotEmpty().WithMessage("Required|LangCode")
            .Must(l => new[] { "az", "ru", "en" }.Contains(l))
            .WithMessage("InvalidLang|LangCode");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Required|Name")
            .MaximumLength(100).WithMessage("MaxLength|Name|100");
    }
}
