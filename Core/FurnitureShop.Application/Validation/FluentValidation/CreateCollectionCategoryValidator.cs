using FluentValidation;
using FurnitureShop.Application.Dtos.CollectionCategory;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateCollectionCategoryValidator : AbstractValidator<CreateCollectionCategoryDto>
{
    public CreateCollectionCategoryValidator()
    {
        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Required|Translations")
            .Must(x => x.Count > 0).WithMessage("MinCount|Translations|1");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.Lang)
                .NotEmpty().WithMessage("Required|LangCode")
                .Must(l => new[] { "az", "ru", "en" }.Contains(l))
                .WithMessage("InvalidLang|LangCode");
            t.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Required|Name")
                .MaximumLength(100).WithMessage("MaxLength|Name|100");
        });
    }
}
