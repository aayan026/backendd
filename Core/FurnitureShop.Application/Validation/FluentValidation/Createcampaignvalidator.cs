using FluentValidation;
using FurnitureShop.Application.Dtos.Campaign;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Required|Images");

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).When(x => x.DiscountPercent.HasValue)
            .WithMessage("GreaterThanZero|DiscountPercent")
            .LessThanOrEqualTo(100).When(x => x.DiscountPercent.HasValue)
            .WithMessage("MaxLength|DiscountPercent|100");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("FutureDate|ScheduledDate");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("FutureDate|ScheduledDate");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Required|Translations")
            .Must(x => x.Count > 0).WithMessage("MinCount|Translations|1");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.Lang)
                .NotEmpty().WithMessage("Required|LangCode")
                .Must(l => new[] { "az", "ru", "en" }.Contains(l))
                .WithMessage("InvalidLang|LangCode");

            t.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Required|Name")
                .MaximumLength(200).WithMessage("MaxLength|Name|200");
        });
    }
}