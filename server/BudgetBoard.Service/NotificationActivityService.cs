using System.Globalization;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public sealed class NotificationActivityService(
    UserDataContext userDataContext,
    IBudgetService budgetService,
    ITransactionCategoryService transactionCategoryService,
    INotificationDispatcher notificationDispatcher,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer,
    ILogger<NotificationActivityService> logger
) : INotificationActivityService
{
    private const int MaxTransactionDetails = 10;
    private ActivityBatch? currentBatch;

    public async Task<INotificationActivityScope> BeginAsync(Guid userId)
    {
        if (currentBatch?.UserId == userId)
        {
            return new ActivityScope(this, currentBatch, isOwner: false);
        }

        var budgetSnapshots = new Dictionary<DateOnly, BudgetSnapshot>();
        var currentMonth = GetMonthStart(nowProvider.Today);
        try
        {
            budgetSnapshots[currentMonth] = await CaptureBudgetSnapshotAsync(
                userId,
                currentMonth
            );
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["NotificationActivityCaptureFailedLog", exception.GetType().Name]
            );
        }

        var batch = new ActivityBatch(userId, budgetSnapshots, currentBatch);
        currentBatch = batch;
        return new ActivityScope(this, batch, isOwner: true);
    }

    public async Task TrackBudgetMonthAsync(Guid userId, DateOnly transactionDate)
    {
        var batch = currentBatch;
        if (batch?.UserId != userId)
        {
            return;
        }

        var month = GetMonthStart(transactionDate);
        if (batch.BudgetSnapshots.ContainsKey(month))
        {
            return;
        }

        try
        {
            batch.BudgetSnapshots[month] = await CaptureBudgetSnapshotAsync(userId, month);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["NotificationActivityCaptureFailedLog", exception.GetType().Name]
            );
        }
    }

    public async Task TrackTransactionCreatedAsync(Guid userId, Transaction transaction)
    {
        if (currentBatch?.UserId == userId)
        {
            currentBatch.Transactions.TryAdd(transaction.ID, transaction);
            return;
        }

        await DispatchTransactionEventsAsync(userId, [transaction]);
    }

    private async Task CompleteAsync(ActivityBatch batch)
    {
        if (batch.IsCompleted)
        {
            return;
        }

        batch.IsCompleted = true;
        try
        {
            foreach (var (month, previousSnapshot) in batch.BudgetSnapshots.OrderBy(entry => entry.Key))
            {
                try
                {
                    var currentSnapshot = await CaptureBudgetSnapshotAsync(batch.UserId, month);
                    await DispatchBudgetWarningsAsync(
                        batch.UserId,
                        previousSnapshot,
                        currentSnapshot
                    );
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        "{LogMessage}",
                        logLocalizer[
                            "NotificationActivityCaptureFailedLog",
                            exception.GetType().Name
                        ]
                    );
                }
            }

            try
            {
                await DispatchTransactionEventsAsync(
                    batch.UserId,
                    batch.Transactions.Values.ToList()
                );
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer["NotificationActivityDispatchFailedLog", exception.GetType().Name]
                );
            }
        }
        finally
        {
            if (ReferenceEquals(currentBatch, batch))
            {
                currentBatch = batch.Previous;
            }
        }
    }

    private async Task<BudgetSnapshot> CaptureBudgetSnapshotAsync(Guid userId, DateOnly month)
    {
        var monthEnd = month.AddMonths(1).AddDays(-1);
        var settings = await userDataContext.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(userSettings => userSettings.UserID == userId);
        var budgets = await budgetService.ReadBudgetsAsync(userId, month);
        var categories = await transactionCategoryService.ReadTransactionCategoriesAsync(userId);
        var categoryTypes = categories
            .GroupBy(category => category.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().CategoryType,
                StringComparer.OrdinalIgnoreCase
            );
        var transactions = await userDataContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.Account!.UserID == userId
                && transaction.Account.HideTransactions == false
                && transaction.Deleted == null
                && transaction.Category != TransactionCategoriesConstants.HideFromBudgetsCategory
                && transaction.Date >= month
                && transaction.Date <= monthEnd
            )
            .Select(transaction => new
            {
                transaction.Category,
                transaction.Subcategory,
                transaction.Amount,
            })
            .ToListAsync();

        var amountsByCategory = new Dictionary<string, decimal>(
            StringComparer.OrdinalIgnoreCase
        );
        foreach (var transaction in transactions)
        {
            var category = transaction.Category ?? string.Empty;
            AddAmount(category, transaction.Amount);
            if (
                !string.IsNullOrEmpty(transaction.Subcategory)
                && !transaction.Subcategory.Equals(category, StringComparison.OrdinalIgnoreCase)
            )
            {
                AddAmount(transaction.Subcategory, transaction.Amount);
            }
        }

        var budgetStates = new Dictionary<Guid, BudgetState>();
        foreach (var budget in budgets)
        {
            var categoryType = categoryTypes.GetValueOrDefault(
                budget.Category,
                TransactionCategoryTypes.Expense
            );
            var signedAmount = amountsByCategory.GetValueOrDefault(budget.Category);
            var spent = categoryType == TransactionCategoryTypes.Income
                ? signedAmount
                : -signedAmount;
            var availableLimit = budget.Limit + budget.Rollover;
            var percentUsed = availableLimit > 0 ? spent / availableLimit * 100 : 0;

            budgetStates[budget.ID] = new BudgetState(
                budget.Category,
                budget.Limit,
                budget.Rollover,
                spent,
                percentUsed
            );
        }

        return new BudgetSnapshot(
            settings?.BudgetWarningThreshold ?? 80,
            budgetStates
        );

        void AddAmount(string category, decimal amount)
        {
            amountsByCategory[category] = amountsByCategory.GetValueOrDefault(category) + amount;
        }
    }

    private async Task DispatchBudgetWarningsAsync(
        Guid userId,
        BudgetSnapshot previous,
        BudgetSnapshot current
    )
    {
        if (
            previous.WarningThreshold <= 0
            || current.WarningThreshold != previous.WarningThreshold
        )
        {
            return;
        }

        foreach (var (budgetId, previousBudget) in previous.Budgets)
        {
            if (
                !current.Budgets.TryGetValue(budgetId, out var currentBudget)
                || previousBudget.Limit != currentBudget.Limit
                || previousBudget.Rollover != currentBudget.Rollover
                || previousBudget.PercentUsed >= previous.WarningThreshold
                || currentBudget.PercentUsed < previous.WarningThreshold
            )
            {
                continue;
            }

            await TryDispatchAsync(
                new NotificationEvent(
                    userId,
                    NotificationEventTypes.BudgetWarning,
                    responseLocalizer["NotificationBudgetWarning", currentBudget.Category],
                    responseLocalizer[
                        "NotificationBudgetWarningDetails",
                        previous.WarningThreshold,
                        FormatAmount(currentBudget.Spent),
                        FormatAmount(currentBudget.Limit + currentBudget.Rollover)
                    ]
                )
            );
        }
    }

    private async Task DispatchTransactionEventsAsync(
        Guid userId,
        IReadOnlyCollection<Transaction> transactions
    )
    {
        var eligibleTransactions = transactions
            .Where(transaction =>
                transaction.Deleted == null
                && !string.Equals(
                    transaction.Category,
                    TransactionCategoriesConstants.TransferCategory,
                    StringComparison.OrdinalIgnoreCase
                )
                && transaction.SourceTransactionLink is null
                && transaction.TargetTransactionLink is null
            )
            .ToList();
        if (eligibleTransactions.Count == 0)
        {
            return;
        }

        var transactionDetails = BuildTransactionDetails(eligibleTransactions);
        await TryDispatchAsync(
            new NotificationEvent(
                userId,
                NotificationEventTypes.TransactionsAdded,
                responseLocalizer["NotificationTransactionsAdded", eligibleTransactions.Count],
                transactionDetails
            )
        );

        var threshold = await userDataContext.UserSettings
            .AsNoTracking()
            .Where(settings => settings.UserID == userId)
            .Select(settings => settings.LargeTransactionThreshold)
            .FirstOrDefaultAsync();
        if (threshold is not > 0)
        {
            return;
        }

        var largeTransactions = eligibleTransactions
            .Where(transaction =>
                transaction.Amount >= threshold.Value || transaction.Amount <= -threshold.Value
            )
            .ToList();
        if (largeTransactions.Count == 0)
        {
            return;
        }

        await TryDispatchAsync(
            new NotificationEvent(
                userId,
                NotificationEventTypes.LargeTransactionsAdded,
                responseLocalizer["NotificationLargeTransactionsAdded", largeTransactions.Count],
                BuildTransactionDetails(largeTransactions)
            )
        );
    }

    private string BuildTransactionDetails(IReadOnlyCollection<Transaction> transactions)
    {
        var details = transactions
            .Take(MaxTransactionDetails)
            .Select(transaction =>
                responseLocalizer[
                    "NotificationTransactionDetail",
                    transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    transaction.MerchantName ?? transaction.Category ?? string.Empty,
                    FormatAmount(transaction.Amount)
                ].Value
            )
            .ToList();

        if (transactions.Count > MaxTransactionDetails)
        {
            details.Add(
                responseLocalizer[
                    "NotificationMoreTransactions",
                    transactions.Count - MaxTransactionDetails
                ].Value
            );
        }

        return string.Join(Environment.NewLine, details);
    }

    private async Task TryDispatchAsync(NotificationEvent notification)
    {
        try
        {
            await notificationDispatcher.DispatchAsync(notification);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["NotificationActivityDispatchFailedLog", exception.GetType().Name]
            );
        }
    }

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.##", CultureInfo.InvariantCulture);

    private static DateOnly GetMonthStart(DateOnly date) => new(date.Year, date.Month, 1);

    private sealed record BudgetSnapshot(
        int WarningThreshold,
        IReadOnlyDictionary<Guid, BudgetState> Budgets
    );

    private sealed record BudgetState(
        string Category,
        decimal Limit,
        decimal Rollover,
        decimal Spent,
        decimal PercentUsed
    );

    private sealed class ActivityBatch(
        Guid userId,
        Dictionary<DateOnly, BudgetSnapshot> budgetSnapshots,
        ActivityBatch? previous
    )
    {
        public Guid UserId { get; } = userId;
        public Dictionary<DateOnly, BudgetSnapshot> BudgetSnapshots { get; } = budgetSnapshots;
        public ActivityBatch? Previous { get; } = previous;
        public Dictionary<Guid, Transaction> Transactions { get; } = [];
        public bool IsCompleted { get; set; }
    }

    private sealed class ActivityScope(
        NotificationActivityService service,
        ActivityBatch batch,
        bool isOwner
    ) : INotificationActivityScope
    {
        public Task CompleteAsync() => isOwner ? service.CompleteAsync(batch) : Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            await CompleteAsync();
        }
    }
}
