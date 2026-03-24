using FluentValidation;
using FurnitureShop.Application.Dtos.Order;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.AddressId)
            .GreaterThan(0)
            .When(x => x.AddressId.HasValue)
            .WithMessage("InvalidId|AddressId");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Required|Items")
            .Must(x => x.Count > 0).WithMessage("MinCount|Items|1");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("GreaterThanZero|Quantity");
            item.RuleFor(x => x)
                .Must(x => x.ProductId.HasValue || x.CollectionId.HasValue)
                .WithMessage("ProductOrCollectionRequired|Item");
        });

        RuleFor(x => x.DeliveryInfo)
            .NotNull().WithMessage("Required|DeliveryInfo");

        When(x => x.DeliveryInfo != null, () =>
        {
            RuleFor(x => x.DeliveryInfo!.ScheduledDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("FutureDate|ScheduledDate");
            RuleFor(x => x.DeliveryInfo!.TimeSlot)
                .NotEmpty().WithMessage("Required|TimeSlot");
        });
    }
}
