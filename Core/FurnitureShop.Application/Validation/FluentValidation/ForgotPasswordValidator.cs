using FluentValidation;
using FurnitureShop.Application.Dtos.Auth;

namespace FurnitureShop.Application.Validation.FluentValidation;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Required|Email")
            .EmailAddress().WithMessage("EmailInvalid|Email");
    }
}