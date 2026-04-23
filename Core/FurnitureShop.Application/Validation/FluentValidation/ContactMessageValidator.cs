using FluentValidation;
using FurnitureShop.Application.Dtos.Contact;

namespace FurnitureShop.Application.Validation.FluentValidation;

public class ContactMessageValidator : AbstractValidator<ContactMessageDto>
{
    public ContactMessageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("ContactNameRequired")
            .MaximumLength(100).WithMessage("ContactNameMaxLength");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ContactEmailRequired")
            .EmailAddress().WithMessage("ContactEmailInvalid");

        When(x => !string.IsNullOrEmpty(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage("ContactPhoneInvalid");
        });

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("ContactSubjectRequired");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("ContactMessageRequired")
            .MaximumLength(2000).WithMessage("ContactMessageMaxLength");
    }
}