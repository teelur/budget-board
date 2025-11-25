using BudgetBoard.Database.Data;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Quartz;

namespace BudgetBoard.WebAPI.Jobs;

[DisallowConcurrentExecution]
public class SyncBackgroundJob(
    ILogger<SyncBackgroundJob> logger,
    UserDataContext userDataContext,
    ISimpleFinService simpleFinService,
    IApplicationUserService applicationUserService,
    INowProvider nowProvider,
    IStringLocalizer<ApiLogStrings> logLocalizer
) : IJob
{
    private readonly ILogger _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly ISimpleFinService _simpleFinService = simpleFinService;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;

    public async Task Execute(IJobExecutionContext context)
    {
        var users = _userDataContext
            .ApplicationUsers.Include(user => user.Accounts)
            .ThenInclude(a => a.Transactions)
            .Include(user => user.Accounts)
            .ThenInclude(a => a.Balances)
            .Include(user => user.Institutions)
            .AsSplitQuery()
            .ToList();

        foreach (var user in users)
        {
            try
            {
                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer["SyncBackgroundJobStartLog", user.Email ?? string.Empty]
                );

                await _simpleFinService.SyncAsync(user.Id);

                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer["SyncBackgroundJobSuccessLog", user.Email ?? string.Empty]
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{LogMessage}",
                    _logLocalizer["SyncBackgroundJobErrorLog", user.Email ?? string.Empty]
                );
            }
        }
    }
}
