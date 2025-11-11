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
                _logger.LogInformation("Syncing SimpleFin data for {user}...", user.Email);

                await _simpleFinService.SyncAsync(user.Id);

                _logger.LogInformation("Sync successful for {user}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SimpleFin data for {user}", user.Email);
            }
        }
    }
}
