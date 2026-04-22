using FluentValidation;
using FurnitureShop.Application.Dtos.Campaign;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignDto>
{
    private static readonly string[] RequiredLangs = { "az", "ru", "en" };

    public CreateCampaignValidator()
    {
        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).When(x => x.DiscountPercent.HasValue)
            .WithMessage("GreaterThanZero|DiscountPercent")
            .LessThanOrEqualTo(100).When(x => x.DiscountPercent.HasValue)
            .WithMessage("MaxLength|DiscountPercent|100");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("FutureDate|ScheduledDate");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("FutureDate|ScheduledDate");

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
