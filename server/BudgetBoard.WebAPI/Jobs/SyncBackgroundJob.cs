using BudgetBoard.Database.Data;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BudgetBoard.WebAPI.Jobs;

[DisallowConcurrentExecution]
public class SyncBackgroundJob(
    ILogger<SyncBackgroundJob> logger,
    UserDataContext userDataContext,
    ISimpleFinService simpleFinService,
    IApplicationUserService applicationUserService,
    INowProvider nowProvider
) : IJob
{
    private readonly ILogger _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly ISimpleFinService _simpleFinService = simpleFinService;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly INowProvider _nowProvider = nowProvider;

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
                if (user.AccessToken == string.Empty)
                {
                    continue;
                }

                _logger.LogInformation("Syncing SimpleFin data for {user}...", user.Email);

                long startDate;
                if (user.LastSync == DateTime.MinValue)
                {
                    // If we haven't synced before, sync the full 90 days of history
                    startDate =
                        ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds()
                        - (Helpers.UNIX_MONTH * 3);
                }
                else
                {
                    var oneMonthAgo =
                        ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds()
                        - Helpers.UNIX_MONTH;
                    var lastSyncWithBuffer =
                        ((DateTimeOffset)user.LastSync).ToUnixTimeSeconds() - Helpers.UNIX_WEEK;

                    startDate = Math.Min(oneMonthAgo, lastSyncWithBuffer);
                }

                await _simpleFinService.SyncAsync(user.Id);

                await _applicationUserService.UpdateApplicationUserAsync(
                    user.Id,
                    new ApplicationUserUpdateRequest { LastSync = _nowProvider.UtcNow }
                );

                _logger.LogInformation("Sync successful for {user}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SimpleFin data for {user}", user.Email);
            }
        }
    }
}
