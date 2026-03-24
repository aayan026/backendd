using FluentValidation;
using FurnitureShop.Application.Dtos.Product;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("GreaterThanZero|Price");

        RuleFor(x => x.PriceExtra)
            .GreaterThan(0)
            .When(x => x.PriceExtra.HasValue)
            .WithMessage("GreaterThanZero|PriceExtra");

        RuleFor(x => x.Stock)
            .GreaterThan(-1).WithMessage("GreaterThanZero|Stock");

        RuleFor(x => x.FurnitureCategoryId)
            .GreaterThan(0).WithMessage("InvalidId|CategoryId");

        RuleFor(x => x.Colors)
            .NotNull().WithMessage("Required|Colors")
            .Must(x => x.Count > 0).WithMessage("MinCount|Colors|1");

        RuleForEach(x => x.Colors).ChildRules(color =>
        {
            color.RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Required|ColorName");
            color.RuleFor(c => c.HexCode)
                .NotEmpty().WithMessage("Required|HexCode")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("InvalidHexCode|HexCode");
        });

        RuleFor(x => x.ImageUrls)
            .NotNull().WithMessage("Required|Images")
            .Must(x => x.Count > 0).WithMessage("MinCount|Images|1");

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
                .MaximumLength(200).WithMessage("MaxLength|Name|200");
        });
    }
}
