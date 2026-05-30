using BudgetBoard.Service.Interfaces;
using Quartz;

namespace BudgetBoard.WebAPI.Jobs;

[DisallowConcurrentExecution]
public class DemoResetJob(ILogger<DemoResetJob> logger, IDemoSeedService demoSeedService) : IJob
{
    private readonly ILogger<DemoResetJob> _logger = logger;
    private readonly IDemoSeedService _demoSeedService = demoSeedService;

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Demo reset job: starting nightly database reset…");
        try
        {
            await _demoSeedService.ResetAndSeedAsync();
            _logger.LogInformation("Demo reset job: completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Demo reset job: failed with an unhandled exception.");
        }
    }
}
