using FluentValidation;
using FurnitureShop.Application.Dtos.Product;

namespace FurnitureShop.Application.Validation.Concrete;

public class ProductTranslationValidator : AbstractValidator<ProductTranslationDto>
{
    public ProductTranslationValidator()
    {
        RuleFor(x => x.Lang)
            .NotEmpty().WithMessage("Required|LangCode")
            .Must(l => new[] { "az", "ru", "en" }.Contains(l))
            .WithMessage("InvalidLang|LangCode");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Required|Name")
            .MaximumLength(200).WithMessage("MaxLength|Name|200");
    }
}
