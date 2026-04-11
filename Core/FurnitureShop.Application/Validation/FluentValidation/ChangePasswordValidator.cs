using FluentValidation;
using FurnitureShop.Application.Dtos.User;

namespace FurnitureShop.Application.Validation.FluentValidation;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Required|CurrentPassword");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Required|Password")
            .MinimumLength(8).WithMessage("MinLength|Password|8")
            .Matches("[A-Z]").WithMessage("PasswordWeak|Password")
            .Matches("[0-9]").WithMessage("PasswordWeak|Password");
    }
}