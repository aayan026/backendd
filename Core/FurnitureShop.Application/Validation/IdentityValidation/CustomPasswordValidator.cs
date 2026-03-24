using FurnitureShop.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.Application.Validation.IdentityValidation;

public class CustomPasswordValidator : IPasswordValidator<AppUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        var errors = new List<IdentityError>();

        if (password!.ToLower().Contains(user.UserName!.ToLower()))
            errors.Add(new IdentityError
            {
                Code        = "PasswordContainsUserName",
                Description = "PasswordContainsUserName"  // açar — handler lokalizasiya edir
            });

        if (password.Contains("1234") || password.Contains("0000") || password.Contains("1111"))
            errors.Add(new IdentityError
            {
                Code        = "PasswordContainsSequential",
                Description = "PasswordContainsSequential"
            });

        var commonPasswords = new[] { "password", "qwerty", "abc123", "123456" };
        if (commonPasswords.Any(p => password.ToLower().Contains(p)))
            errors.Add(new IdentityError
            {
                Code        = "PasswordTooCommon",
                Description = "PasswordTooCommon"
            });

        if (!string.IsNullOrEmpty(user.Name) &&
            password.ToLower().Contains(user.Name.ToLower()))
            errors.Add(new IdentityError
            {
                Code        = "PasswordContainsName",
                Description = "PasswordContainsName"
            });

        return errors.Any()
            ? Task.FromResult(IdentityResult.Failed(errors.ToArray()))
            : Task.FromResult(IdentityResult.Success);
    }
}
