using FluentValidation;
using FurnitureShop.Application.Dtos.Address;
using FurnitureShop.Application.Validation;

namespace FurnitureShop.Application.Validation.Concrete;

public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Etiket"))
            .MaximumLength(100).WithMessage(ValidationMessages.Get("az", "MaxLength", "Etiket", 100));

        RuleFor(x => x.ContactName)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Əlaqə adı"))
            .MaximumLength(150).WithMessage(ValidationMessages.Get("az", "MaxLength", "Əlaqə adı", 150));

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Telefon"))
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage(ValidationMessages.Get("az", "PhoneInvalid"));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Şəhər"));

        RuleFor(x => x.AddressLine)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Ünvan"))
            .MaximumLength(500).WithMessage(ValidationMessages.Get("az", "MaxLength", "Ünvan", 500));
    }
}
