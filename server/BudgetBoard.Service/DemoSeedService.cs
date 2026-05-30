using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class DemoSeedService(
    ILogger<DemoSeedService> logger,
    UserDataContext userDataContext,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IStringLocalizer<LogStrings> logLocalizer,
    ITransactionService transactionService
) : IDemoSeedService
{
    private const int MonthsOfData = 6;
    private readonly ILogger<DemoSeedService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;
    private readonly ITransactionService _transactionService = transactionService;

    private static readonly (
        string Category,
        string Subcategory,
        string Merchant,
        decimal Min,
        decimal Max,
        bool IsExpense
    )[] TransactionPool =
    [
        ("Income", "Paycheck", "Employer Direct Deposit", 2400m, 3200m, false),
        ("Food & Dining", "Groceries", "Whole Foods Market", 40m, 180m, true),
        ("Food & Dining", "Groceries", "Trader Joe's", 25m, 110m, true),
        ("Food & Dining", "Coffee Shops", "Starbucks", 4m, 12m, true),
        ("", "", "Square *Unknown Vendor", 10m, 60m, true),
        ("Food & Dining", "Restaurants", "Chipotle", 8m, 22m, true),
        ("Food & Dining", "Restaurants", "Local Bistro", 18m, 65m, true),
        ("Auto & Transport", "Gas & Fuel", "Shell", 30m, 80m, true),
        ("Auto & Transport", "Ride Share", "Uber", 8m, 35m, true),
        ("Auto & Transport", "Parking", "ParkMobile", 5m, 25m, true),
        ("Shopping", "Household", "Amazon", 12m, 240m, true),
        ("", "", "ACH Debit 4837291", 20m, 150m, true),
        ("Shopping", "Clothing", "Target", 15m, 120m, true),
        ("Shopping", "Electronics & Software", "Best Buy", 25m, 600m, true),
        ("Entertainment", "Movies", "Netflix", 15m, 20m, true),
        ("Entertainment", "Music", "Spotify", 9m, 11m, true),
        ("Entertainment", "Movies", "AMC Theatres", 10m, 28m, true),
        ("Bills & Utilities", "Utilities", "City Electric Co.", 60m, 160m, true),
        ("Bills & Utilities", "Internet", "Comcast", 55m, 80m, true),
        ("", "", "Online Purchase 9921", 5m, 80m, true),
        ("Bills & Utilities", "Mobile Phone", "Verizon", 45m, 90m, true),
        ("Health & Fitness", "Pharmacy", "CVS Pharmacy", 8m, 75m, true),
        ("Health & Fitness", "Gym", "LA Fitness", 25m, 45m, true),
    ];

    /// <inheritdoc />
    public async Task ResetAndSeedAsync()
    {
        _logger.LogInformation("{LogMessage}", _logLocalizer["DemoResetDeletingUsersLog"].Value);
        await DeleteAllUsersAsync();

        _logger.LogInformation("{LogMessage}", _logLocalizer["DemoResetSeedingLog"].Value);
        await SeedDemoUserAsync();

        _logger.LogInformation("{LogMessage}", _logLocalizer["DemoResetCompleteLog"].Value);
    }

    /// <summary>
    /// Deletes all users and their corresponding data.
    /// </summary>
    /// <remarks>
    /// Deleting users will automatically delete all related institutions, accounts, transactions, budgets, etc.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task DeleteAllUsersAsync()
    {
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError(
                    _logLocalizer["DemoResetDeleteUserFailedLog"].Value,
                    user.Email,
                    errors
                );
            }
        }
    }

    /// <summary>
    /// Seeds a single demo user and populates it with institutions, accounts, transactions, budgets, and widget settings.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task SeedDemoUserAsync()
    {
        // I'm just going to hardcode these values, since it's extremely unlikely anyone would want to override this.
        var demoEmail = "demo@example.com";
        var demoPassword = "demo";

        var user = new ApplicationUser
        {
            // Fixed GUID so that login sessions and tokens remain valid after a demo reset.
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            UserName = demoEmail,
            Email = demoEmail,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, demoPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError(_logLocalizer["DemoResetCreateUserFailedLog"].Value, errors);
            return;
        }

        await SeedUserDataAsync(user);
    }

    /// <summary>
    /// Seeds institutions, accounts, transactions, budgets, and widget settings for the provided user.
    /// </summary>
    /// <param name="user">
    /// The user for whom to seed institutions, accounts, transactions, budgets, and widget settings.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task SeedUserDataAsync(ApplicationUser user)
    {
        var rng = new Random();

        _userDataContext.UserSettings.Add(new UserSettings { UserID = user.Id });

        SeedUserAccounts(user);
        await _userDataContext.SaveChangesAsync();

        var checking = _userDataContext.Accounts.First(a =>
            a.UserID == user.Id && a.Name == "Checking"
        );
        await SeedAccountDataAsync(
            user.Id,
            checking.ID,
            rng,
            initialBalance: 4_200m,
            includeIncome: true,
            expenseFrequencyPerMonth: 18
        );

        var savings = _userDataContext.Accounts.First(a =>
            a.UserID == user.Id && a.Name == "Savings"
        );
        await SeedAccountDataAsync(
            user.Id,
            savings.ID,
            rng,
            initialBalance: 12_500m,
            includeIncome: false,
            expenseFrequencyPerMonth: 0,
            savingsTransferAmount: 400m
        );

        var creditCard = _userDataContext.Accounts.First(a =>
            a.UserID == user.Id && a.Name == "Visa Rewards"
        );
        await SeedAccountDataAsync(
            user.Id,
            creditCard.ID,
            rng,
            initialBalance: -620m,
            includeIncome: false,
            expenseFrequencyPerMonth: 14
        );

        var investment = _userDataContext.Accounts.First(a =>
            a.UserID == user.Id && a.Name == "Brokerage"
        );
        await SeedAccountDataAsync(
            user.Id,
            investment.ID,
            rng,
            initialBalance: 31_000m,
            includeIncome: false,
            expenseFrequencyPerMonth: 0,
            monthlyGrowthRate: 0.007m
        );

        SeedBudgetData(user);
        SeedWidgetSettingsData(user);

        await _userDataContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds institutions and accounts for the provided user.
    /// </summary>
    /// <param name="user">
    /// The user for whom to seed institutions and accounts.
    /// </param>
    private void SeedUserAccounts(ApplicationUser user)
    {
        var greenfieldBank = new Institution
        {
            Name = "Greenfield Bank",
            Index = 0,
            UserID = user.Id,
        };
        var summitCredit = new Institution
        {
            Name = "Summit Credit Union",
            Index = 1,
            UserID = user.Id,
        };

        _userDataContext.Institutions.AddRange(greenfieldBank, summitCredit);

        var checking = new Account
        {
            Name = "Checking",
            Type = "Checking",
            InstitutionID = greenfieldBank.ID,
            Source = "Manual",
            Index = 0,
            UserID = user.Id,
        };
        var savings = new Account
        {
            Name = "Savings",
            Type = "Savings",
            InstitutionID = greenfieldBank.ID,
            Source = "Manual",
            Index = 1,
            UserID = user.Id,
        };
        var creditCard = new Account
        {
            Name = "Visa Rewards",
            Type = "Credit Card",
            InstitutionID = summitCredit.ID,
            Source = "Manual",
            Index = 2,
            UserID = user.Id,
        };
        var investment = new Account
        {
            Name = "Brokerage",
            Type = "Investment",
            InstitutionID = summitCredit.ID,
            Source = "Manual",
            Index = 3,
            UserID = user.Id,
        };

        _userDataContext.Accounts.AddRange(checking, savings, creditCard, investment);
    }

    /// <summary>
    /// Generates transactions for a single account over 6 months. Balances are managed automatically by the transaction service.
    /// </summary>
    private async Task SeedAccountDataAsync(
        Guid userId,
        Guid accountId,
        Random rng,
        decimal initialBalance,
        bool includeIncome,
        int expenseFrequencyPerMonth,
        decimal savingsTransferAmount = 0m,
        decimal monthlyGrowthRate = 0m
    )
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Seed an initial balance so the transaction service can build on it.
        var firstMonth = today.AddMonths(-(MonthsOfData - 1));
        var initialBalanceDate = new DateOnly(firstMonth.Year, firstMonth.Month, 1).AddDays(-1);
        _userDataContext.Balances.Add(
            new Balance
            {
                Amount = initialBalance,
                Date = initialBalanceDate,
                AccountID = accountId,
            }
        );
        await _userDataContext.SaveChangesAsync();

        // Used only to compute monthly investment growth; the service tracks the real balance.
        decimal growthBase = initialBalance;

        for (int monthOffset = MonthsOfData - 1; monthOffset >= 0; monthOffset--)
        {
            var month = today.AddMonths(-monthOffset);
            var firstDay = new DateOnly(month.Year, month.Month, 1);
            var lastDay = new DateOnly(
                month.Year,
                month.Month,
                DateTime.DaysInMonth(month.Year, month.Month)
            );
            // Don't generate transactions in the future
            var endDay = monthOffset == 0 ? today : lastDay;

            // Paycheck on 1st and 15th
            if (includeIncome)
            {
                foreach (var paydayOffset in new[] { 0, 14 })
                {
                    var transactionDate = firstDay.AddDays(paydayOffset);
                    if (transactionDate > endDay)
                        continue;

                    var income = TransactionPool.First(t => t.Category == "Income");
                    var amount = GenerateRandomDecimal(rng, income.Min, income.Max);

                    await _transactionService.CreateTransactionAsync(
                        userId,
                        new TransactionCreateRequest
                        {
                            Amount = amount,
                            Date = transactionDate,
                            Category = income.Category,
                            Subcategory = income.Subcategory,
                            MerchantName = income.Merchant,
                            Source = "Manual",
                            AccountID = accountId,
                        }
                    );
                }
            }

            // Monthly savings transfer on the 2nd
            if (savingsTransferAmount > 0)
            {
                var transferDate = firstDay.AddDays(2);
                if (transferDate <= endDay)
                {
                    await _transactionService.CreateTransactionAsync(
                        userId,
                        new TransactionCreateRequest
                        {
                            Amount = savingsTransferAmount,
                            Date = transferDate,
                            Category = "Transfer",
                            Subcategory = "",
                            MerchantName = "Transfer from Checking",
                            Source = "Manual",
                            AccountID = accountId,
                        }
                    );
                }
            }

            // Monthly growth (e.g. investment returns)
            if (monthlyGrowthRate > 0)
            {
                var growthAmount = Math.Round(growthBase * monthlyGrowthRate, 2);
                var growthDate = firstDay.AddDays(rng.Next(1, 5));
                if (growthDate <= endDay)
                {
                    await _transactionService.CreateTransactionAsync(
                        userId,
                        new TransactionCreateRequest
                        {
                            Amount = growthAmount,
                            Date = growthDate,
                            Category = "Income",
                            Subcategory = "Interest Income",
                            MerchantName = "Market Returns",
                            Source = "Manual",
                            AccountID = accountId,
                        }
                    );
                    growthBase += growthAmount;
                }
            }

            // Expenses
            if (expenseFrequencyPerMonth > 0)
            {
                var expensePool = TransactionPool.Where(t => t.IsExpense).ToArray();

                int transactionCount = rng.Next(
                    (int)(expenseFrequencyPerMonth * 0.7),
                    (int)(expenseFrequencyPerMonth * 1.3) + 1
                );
                for (int i = 0; i < transactionCount; i++)
                {
                    var transactionDate = GenerateRandomDate(rng, firstDay, endDay);
                    var template = expensePool[rng.Next(expensePool.Length)];
                    var amount = -GenerateRandomDecimal(rng, template.Min, template.Max);

                    await _transactionService.CreateTransactionAsync(
                        userId,
                        new TransactionCreateRequest
                        {
                            Amount = amount,
                            Date = transactionDate,
                            Category = template.Category,
                            Subcategory = template.Subcategory,
                            MerchantName = template.Merchant,
                            Source = "Manual",
                            AccountID = accountId,
                        }
                    );
                }
            }
        }
    }

    /// <summary>
    /// Seeds budget limits for the provided user for the current month.
    /// </summary>
    /// <param name="user">
    /// The user for whom to seed budget data.
    /// </param>
    private void SeedBudgetData(ApplicationUser user)
    {
        var today = DateTime.UtcNow;
        var budgetMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var budgets = new (string Category, decimal Limit)[]
        {
            ("Food & Dining", 500m),
            ("Shopping", 300m),
            ("Auto & Transport", 200m),
            ("Entertainment", 100m),
            ("Bills & Utilities", 250m),
            ("Health & Fitness", 150m),
        };

        foreach (var (category, limit) in budgets)
        {
            _userDataContext.Budgets.Add(
                new Budget
                {
                    Date = budgetMonth,
                    Category = category,
                    Limit = limit,
                    UserID = user.Id,
                }
            );
        }
    }

    /// <summary>
    /// Seeds default widget settings for the provided user.
    /// </summary>
    /// <param name="user">
    /// The user for whom to seed widget settings.
    /// </param>
    private void SeedWidgetSettingsData(ApplicationUser user)
    {
        foreach (var layout in WidgetSettingsHelpers.DefaultLayouts)
        {
            _userDataContext.WidgetSettings.Add(
                new WidgetSettings
                {
                    WidgetType = layout.WidgetType,
                    LgX = layout.LgX,
                    LgY = layout.LgY,
                    LgW = layout.LgW,
                    LgH = layout.LgH,
                    SmY = layout.SmY,
                    SmH = layout.SmH,
                    UserID = user.Id,
                }
            );
        }
    }

    private static decimal GenerateRandomDecimal(Random rng, decimal min, decimal max)
    {
        var range = (double)(max - min);
        return Math.Round(min + (decimal)(rng.NextDouble() * range), 2);
    }

    private static DateOnly GenerateRandomDate(Random rng, DateOnly from, DateOnly to)
    {
        var range = to.DayNumber - from.DayNumber;
        if (range <= 0)
            return from;
        return from.AddDays(rng.Next(0, range + 1));
    }
}
