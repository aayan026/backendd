using FluentValidation;
using FurnitureShop.Application.Dtos.Auth;

namespace FurnitureShop.Application.Validation.Concrete;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Required|Name")
            .MinimumLength(3).WithMessage("MinLength|Name|3")
            .MaximumLength(50).WithMessage("MaxLength|Name|50");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Required|Surname")
            .MaximumLength(50).WithMessage("MaxLength|Surname|50");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Required|Email")
            .EmailAddress().WithMessage("EmailInvalid|Email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Required|Password")
            .MinimumLength(8).WithMessage("MinLength|Password|8")
            .Matches("[A-Z]").WithMessage("PasswordWeak|Password")
            .Matches("[0-9]").WithMessage("PasswordWeak|Password");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Required|ConfirmPassword")
            .Equal(x => x.Password).WithMessage("PasswordConfirmMismatch");

        When(x => !string.IsNullOrEmpty(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage("PhoneInvalid|Phone");
        });
    }
}
