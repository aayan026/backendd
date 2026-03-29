using FluentValidation;
using FurnitureShop.Application.Dtos.DiscountCode;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateDiscountCodeValidator : AbstractValidator<CreateDiscountCodeDto>
{
    public CreateDiscountCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Required|Name")
            .MinimumLength(3).WithMessage("MinLength|Name|3")
            .MaximumLength(50).WithMessage("MaxLength|Name|50")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Kod yalnız böyük hərf, rəqəm, _ və - ehtiva edə bilər");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Endirim tipi yanlışdır (Percent=0, Fixed=1)");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("GreaterThanZero|Value");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.Type == DiscountType.Percent)
            .WithMessage("Faiz 100-dən böyük ola bilməz");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0).When(x => x.MinOrderAmount.HasValue)
            .WithMessage("GreaterThanZero|MinOrderAmount");

        RuleFor(x => x.MaxUses)
            .GreaterThan(0).When(x => x.MaxUses.HasValue)
            .WithMessage("GreaterThanZero|MaxUses");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue)
            .WithMessage("FutureDate|ExpiresAt");
    }
}
