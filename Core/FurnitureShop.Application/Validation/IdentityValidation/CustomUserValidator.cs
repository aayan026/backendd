using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.Application.Validation.IdentityValidation;

public class CustomUserValidator : IUserValidator<AppUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        var errors = new List<IdentityError>();

   
        if (string.IsNullOrWhiteSpace(user.Name))
            errors.Add(new IdentityError
            {
                Code        = "NameRequired",
                Description = "Required|Name"
            });

        if (string.IsNullOrWhiteSpace(user.Surname))
            errors.Add(new IdentityError
            {
                Code        = "SurnameRequired",
                Description = "Required|Surname"
            });

        return errors.Any()
            ? IdentityResult.Failed(errors.ToArray())
            : IdentityResult.Success;
    }
}
