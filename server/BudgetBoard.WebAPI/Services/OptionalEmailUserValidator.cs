using System.ComponentModel.DataAnnotations;
using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetBoard.WebAPI.Services;

public sealed class OptionalEmailUserValidator : IUserValidator<ApplicationUser>
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public async Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user
    )
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        var email = await manager.GetEmailAsync(user);
        if (string.IsNullOrWhiteSpace(email))
        {
            return IdentityResult.Success;
        }

        var errors = new List<IdentityError>();
        if (!EmailAddressValidator.IsValid(email))
        {
            errors.Add(manager.ErrorDescriber.InvalidEmail(email));
        }
        else
        {
            var existingUser = await manager.FindByEmailAsync(email);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                errors.Add(manager.ErrorDescriber.DuplicateEmail(email));
            }
        }

        return errors.Count == 0
            ? IdentityResult.Success
            : IdentityResult.Failed(errors.ToArray());
    }
}
