using FluentValidation;
using FurnitureShop.Application.Dtos.Order;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("GreaterThanZero|Quantity");

        RuleFor(x => x)
            .Must(x => x.ProductId.HasValue || x.CollectionId.HasValue)
            .WithMessage("ProductOrCollectionRequired|Item");
    }
}
