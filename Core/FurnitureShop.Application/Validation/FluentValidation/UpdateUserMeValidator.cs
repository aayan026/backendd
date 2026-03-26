using FluentValidation;
using FurnitureShop.Application.Dtos.User;
using FurnitureShop.Application.Validation;

namespace FurnitureShop.Application.Validation.Concrete;

public class UpdateUserMeValidator : AbstractValidator<UpdateUserMeDto>
{
    public UpdateUserMeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Ad"))
            .MaximumLength(100).WithMessage(ValidationMessages.Get("az", "MaxLength", "Ad", 100));

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage(ValidationMessages.Get("az", "Required", "Soyad"))
            .MaximumLength(100).WithMessage(ValidationMessages.Get("az", "MaxLength", "Soyad", 100));

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage(ValidationMessages.Get("az", "PhoneInvalid"));
        });
    }
}
