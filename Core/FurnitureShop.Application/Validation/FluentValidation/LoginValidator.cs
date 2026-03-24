using FluentValidation;
using FurnitureShop.Application.Dtos.Auth;

namespace FurnitureShop.Application.Validation.Concrete;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Required|Email")
            .EmailAddress().WithMessage("EmailInvalid|Email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Required|Password");
    }
}
