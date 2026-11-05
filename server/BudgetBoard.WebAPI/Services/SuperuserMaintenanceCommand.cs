using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class SuperuserMaintenanceCommand(
    ISuperuserService superuserService,
    IStringLocalizer<ApiResponseStrings> localizer
)
{
    public async Task<int> RunAsync(string[] arguments, CancellationToken cancellationToken = default)
    {
        if (arguments.Length == 0 || arguments[0] is "--help" or "-h")
        {
            PrintUsage(localizer);
            return 0;
        }

        var command = arguments[0];
        if (
            command is not "reset-superuser"
            and not "bootstrap-superuser"
            || arguments.Length != 3
            || arguments[1] != "--username"
            || string.IsNullOrWhiteSpace(arguments[2])
        )
        {
            Console.Error.WriteLine(
                localizer["AdminCommandUsage"].Value
            );
            return 2;
        }

        SuperuserOperationResult result =
            command == "reset-superuser"
                ? await superuserService.ResetSuperuserFromCommandAsync(
                    arguments[2],
                    cancellationToken
                )
                : await superuserService.BootstrapSuperuserFromCommandAsync(
                    arguments[2],
                    cancellationToken
                );

        if (!result.Succeeded)
        {
            Console.Error.WriteLine(
                localizer["AdminCommandFailed", result.ErrorCode ?? string.Empty].Value
            );
            return 1;
        }

        if (result.TemporaryPassword is not null)
        {
            Console.WriteLine(
                localizer["TemporaryPassword", result.TemporaryPassword].Value
            );
            Console.WriteLine(localizer["PasswordMustBeChangedNextSignIn"].Value);
        }
        else
        {
            Console.WriteLine(
                localizer["SuperuserBootstrapped", result.User?.Username ?? string.Empty].Value
            );
        }

        return 0;
    }

    private static void PrintUsage(IStringLocalizer<ApiResponseStrings> localizer)
    {
        Console.WriteLine(localizer["AdminCommandUsage"].Value);
    }
}
