using FluentValidation;
using FurnitureShop.Application.Dtos.Auth;

namespace FurnitureShop.Application.Validation.FluentValidation;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Required|Email")
            .EmailAddress().WithMessage("EmailInvalid|Email");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Required|Token");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Required|Password")
            .MinimumLength(8).WithMessage("MinLength|Password|8")
            .Matches("[A-Z]").WithMessage("PasswordWeak|Password")
            .Matches("[0-9]").WithMessage("PasswordWeak|Password");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Required|ConfirmPassword")
            .Equal(x => x.NewPassword).WithMessage("PasswordConfirmMismatch");
    }
}