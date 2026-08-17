using BudgetBoard.Service.Interfaces;

namespace BudgetBoard.WebAPI.Jobs;

public class TransactionImportWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TransactionImportWorker> logger
) : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var importService =
                    scope.ServiceProvider.GetRequiredService<ITransactionImportService>();
                var processedJob = await importService.ProcessNextAsync(stoppingToken);

                if (!processedJob)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Transaction import worker iteration failed");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
