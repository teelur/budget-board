using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class RecurringRuleServiceTests
{
    [Fact]
    public void GetOccurrences_ShouldHonorCadenceAnchorsAndMonthBoundaries()
    {
        var weeklyRule = CreateRule(
            RecurringCadence.Weekly,
            new DateOnly(2026, 8, 3)
        );
        var biweeklyRule = CreateRule(
            RecurringCadence.Biweekly,
            new DateOnly(2026, 8, 3)
        );
        var monthlyRule = CreateRule(
            RecurringCadence.Monthly,
            new DateOnly(2026, 1, 31)
        );
        var yearlyRule = CreateRule(
            RecurringCadence.Yearly,
            new DateOnly(2024, 2, 29)
        );

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(weeklyRule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31))
            .Should()
            .Equal(
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 17),
                new DateOnly(2026, 8, 24),
                new DateOnly(2026, 8, 31)
            );
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(biweeklyRule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31))
            .Should()
            .Equal(new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 31));
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(monthlyRule, new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 31))
            .Should()
            .Equal(new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 31));
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(yearlyRule, new DateOnly(2027, 1, 1), new DateOnly(2028, 12, 31))
            .Should()
            .Equal(new DateOnly(2027, 2, 28), new DateOnly(2028, 2, 29));
    }

    [Fact]
    public async Task ReadForecastAsync_ShouldUseAutomaticMedianAndSkipMatchedOccurrences()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = new RecurringRule
        {
            UserID = helper.demoUser.Id,
            AccountID = account.ID,
            Account = account,
            MerchantName = "Rent",
            Category = "Housing",
            Subcategory = "Rent",
            Cadence = RecurringCadence.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            IsActive = true,
            AmountMode = RecurringAmountMode.Automatic,
            Amount = 100,
        };
        var historicalTransactions = new[] { 100m, 120m, 140m }.Select(
            (amount, index) => new Transaction
            {
                Amount = amount,
                Date = new DateOnly(2026, index + 1, 1),
                MerchantName = "Rent",
                Category = "Housing",
                Subcategory = "Rent",
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                RecurringRule = rule,
            }
        );
        rule.Transactions = historicalTransactions.ToList();
        helper.UserDataContext.RecurringRules.Add(rule);
        await helper.UserDataContext.SaveChangesAsync();

        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var forecast = await service.ReadForecastAsync(
            helper.demoUser.Id,
            new DateOnly(2026, 8, 1)
        );

        forecast.Should().ContainSingle();
        forecast[0].Date.Should().Be(new DateOnly(2026, 8, 1));
        forecast[0].Amount.Should().Be(120);

        var matchedTransaction = new Transaction
        {
            Amount = 120,
            Date = new DateOnly(2026, 8, 1),
            MerchantName = "Rent",
            Category = "Housing",
            Subcategory = "Rent",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
            RecurringRule = rule,
        };
        helper.UserDataContext.Transactions.Add(matchedTransaction);
        await helper.UserDataContext.SaveChangesAsync();

        var suppressedForecast = await service.ReadForecastAsync(
            helper.demoUser.Id,
            new DateOnly(2026, 8, 1)
        );

        suppressedForecast.Should().BeEmpty();
    }

    [Fact]
    public async Task MatchTransactionAsync_ShouldMatchNormalizedFieldsWithinAmountTolerance()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(
            helper,
            account,
            merchantName: "Acme Coffee",
            category: "Food",
            subcategory: "Coffee",
            amount: 100
        );
        var transaction = new Transaction
        {
            Amount = 119,
            Date = new DateOnly(2026, 8, 4),
            MerchantName = " ACME-coffee ",
            Category = " food ",
            Subcategory = "COFFEE",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().Be(rule.ID);
    }

    [Fact]
    public async Task MatchTransactionAsync_ShouldLeaveAmbiguousMatchesUnassigned()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        AddRule(helper, account, amount: 100);
        AddRule(helper, account, amount: 100);
        var transaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 3),
            MerchantName = "Merchant",
            Category = "Category",
            Subcategory = "Subcategory",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().BeNull();
    }

    [Fact]
    public async Task ReadRecurringRulesAsync_ShouldOnlyReturnRulesOwnedByUser()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        AddRule(helper, account);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var otherUserRules = await service.ReadRecurringRulesAsync(Guid.NewGuid());
        var userRules = await service.ReadRecurringRulesAsync(helper.demoUser.Id);

        otherUserRules.Should().BeEmpty();
        userRules.Should().ContainSingle();
    }

    private static RecurringRuleService CreateService(TestHelper helper, DateOnly today)
    {
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);

        return new RecurringRuleService(
            Mock.Of<ILogger<IRecurringRuleService>>(),
            helper.UserDataContext,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
    }

    private static Account AddAccount(TestHelper helper)
    {
        var account = new Account
        {
            Name = "Checking",
            InstitutionID = Guid.NewGuid(),
            Type = "checking",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();
        return account;
    }

    private static RecurringRule AddRule(
        TestHelper helper,
        Account account,
        string merchantName = "Merchant",
        string category = "Category",
        string subcategory = "Subcategory",
        decimal amount = 100
    )
    {
        var rule = new RecurringRule
        {
            UserID = helper.demoUser.Id,
            AccountID = account.ID,
            Account = account,
            MerchantName = merchantName,
            Category = category,
            Subcategory = subcategory,
            Cadence = RecurringCadence.Monthly,
            StartDate = new DateOnly(2026, 8, 1),
            IsActive = true,
            AmountMode = RecurringAmountMode.Fixed,
            Amount = amount,
        };
        helper.UserDataContext.RecurringRules.Add(rule);
        helper.UserDataContext.SaveChanges();
        return rule;
    }

    private static RecurringRule CreateRule(RecurringCadence cadence, DateOnly startDate) =>
        new()
        {
            UserID = Guid.NewGuid(),
            AccountID = Guid.NewGuid(),
            Cadence = cadence,
            StartDate = startDate,
            IsActive = true,
            Amount = 100,
        };
}