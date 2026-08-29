using System.Text.Json;
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
    #region GetOccurrences
    [Fact]
    public void GetOccurrences_ShouldHonorCadenceAnchorsAndMonthBoundaries()
    {
        var weeklyRule = CreateRule(RecurringCadenceUnitValues.Week, 1, new DateOnly(2026, 8, 3));
        var biweeklyRule = CreateRule(RecurringCadenceUnitValues.Week, 2, new DateOnly(2026, 8, 3));
        var monthlyRule = CreateRule(
            RecurringCadenceUnitValues.Month,
            1,
            new DateOnly(2026, 1, 31)
        );
        var yearlyRule = CreateRule(RecurringCadenceUnitValues.Year, 1, new DateOnly(2024, 2, 29));

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
    public void GetOccurrences_ShouldSupportArbitraryIntervals()
    {
        var dailyRule = CreateRule(RecurringCadenceUnitValues.Day, 3, new DateOnly(2026, 8, 1));
        var weeklyRule = CreateRule(RecurringCadenceUnitValues.Week, 2, new DateOnly(2026, 8, 3));
        var monthlyRule = CreateRule(
            RecurringCadenceUnitValues.Month,
            2,
            new DateOnly(2026, 1, 31)
        );
        var yearlyRule = CreateRule(RecurringCadenceUnitValues.Year, 2, new DateOnly(2024, 2, 29));

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(dailyRule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10))
            .Should()
            .Equal(
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 4),
                new DateOnly(2026, 8, 7),
                new DateOnly(2026, 8, 10)
            );
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(weeklyRule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31))
            .Should()
            .Equal(new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 31));
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(monthlyRule, new DateOnly(2026, 1, 1), new DateOnly(2026, 7, 31))
            .Should()
            .Equal(
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 5, 31),
                new DateOnly(2026, 7, 31)
            );
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(yearlyRule, new DateOnly(2024, 1, 1), new DateOnly(2030, 12, 31))
            .Should()
            .Equal(
                new DateOnly(2024, 2, 29),
                new DateOnly(2026, 2, 28),
                new DateOnly(2028, 2, 29),
                new DateOnly(2030, 2, 28)
            );
    }

    [Fact]
    public void GetOccurrences_ShouldSupportMultipleOccurrencesPerUnit()
    {
        var dailyRule = CreateRule(
            RecurringCadenceUnitValues.Day,
            1,
            new DateOnly(2026, 8, 1),
            RecurringCadenceModeValues.PerUnit
        );
        var twiceWeeklyRule = CreateRule(
            RecurringCadenceUnitValues.Week,
            2,
            new DateOnly(2026, 8, 3),
            RecurringCadenceModeValues.PerUnit
        );
        var twiceMonthlyRule = CreateRule(
            RecurringCadenceUnitValues.Month,
            2,
            new DateOnly(2026, 1, 31),
            RecurringCadenceModeValues.PerUnit
        );
        var thriceYearlyRule = CreateRule(
            RecurringCadenceUnitValues.Year,
            3,
            new DateOnly(2024, 2, 29),
            RecurringCadenceModeValues.PerUnit
        );

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(dailyRule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 2))
            .Should()
            .Equal(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 2));
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(twiceWeeklyRule, new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 17))
            .Should()
            .Equal(
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 8, 6),
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 13),
                new DateOnly(2026, 8, 17)
            );
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(twiceMonthlyRule, new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31))
            .Should()
            .Equal(
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 2, 14),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 15),
                new DateOnly(2026, 3, 31)
            );
        RecurringRuleOccurrenceCalculator
            .GetOccurrences(thriceYearlyRule, new DateOnly(2024, 1, 1), new DateOnly(2025, 12, 31))
            .Should()
            .Equal(
                new DateOnly(2024, 2, 29),
                new DateOnly(2024, 6, 30),
                new DateOnly(2024, 10, 30),
                new DateOnly(2025, 2, 28),
                new DateOnly(2025, 6, 29),
                new DateOnly(2025, 10, 29)
            );
    }

    [Fact]
    public void GetOccurrences_ShouldReturnEmptyWhenRangeIsReversed()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Day, 1, new DateOnly(2026, 8, 1));

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 1))
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void GetOccurrences_ShouldReturnEmptyForInactiveRules()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Day, 1, new DateOnly(2026, 8, 1));
        rule.IsActive = false;

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10))
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void GetOccurrences_ShouldReturnEmptyWhenRuleEndedBeforeRange()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Day, 1, new DateOnly(2026, 7, 1));
        rule.EndDate = new DateOnly(2026, 7, 31);

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10))
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void GetOccurrences_ShouldReturnEmptyWhenRuleStartsAfterRange()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Day, 1, new DateOnly(2026, 9, 1));

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31))
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void GetOccurrences_ShouldRejectUnsupportedUnitInDispatch()
    {
        var act = () =>
            RecurringRuleOccurrenceCalculator.GetOccurrences(
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                new RecurringCadence
                {
                    Version = 1,
                    Unit = "Fortnight",
                    Interval = 1,
                }
            );

        act.Should()
            .Throw<RecurringCadenceValidationException>()
            .WithMessage("Cadence unit is not supported.");
    }

    [Fact]
    public void GetOccurrences_ShouldFilterWeeklyPerUnitOccurrencesToRange()
    {
        var rule = CreateRule(
            RecurringCadenceUnitValues.Week,
            2,
            new DateOnly(2026, 8, 3),
            RecurringCadenceModeValues.PerUnit
        );

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2026, 8, 4), new DateOnly(2026, 8, 6))
            .Should()
            .Equal(new DateOnly(2026, 8, 6));
    }

    [Fact]
    public void GetOccurrences_ShouldFilterYearlyPerUnitOccurrencesToRange()
    {
        var rule = CreateRule(
            RecurringCadenceUnitValues.Year,
            3,
            new DateOnly(2024, 2, 29),
            RecurringCadenceModeValues.PerUnit
        );

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2024, 3, 1), new DateOnly(2024, 9, 1))
            .Should()
            .Equal(new DateOnly(2024, 6, 30));
    }

    [Fact]
    public void GetOccurrences_ShouldFilterYearlyOccurrencesToRange()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Year, 1, new DateOnly(2024, 2, 29));

        RecurringRuleOccurrenceCalculator
            .GetOccurrences(rule, new DateOnly(2027, 3, 1), new DateOnly(2028, 2, 29))
            .Should()
            .Equal(new DateOnly(2028, 2, 29));
    }

    #endregion
    #region RecurringCadenceSerializer
    [Fact]
    public void RecurringCadenceSerializer_ShouldValidateAndRoundTripV1Definition()
    {
        var cadence = new RecurringCadence
        {
            Version = 1,
            Unit = RecurringCadenceUnitValues.Month,
            Interval = 2,
        };

        var serialized = RecurringCadenceSerializer.Serialize(cadence);
        var deserialized = RecurringCadenceSerializer.Deserialize(serialized);

        deserialized.Version.Should().Be(1);
        deserialized.Unit.Should().Be(RecurringCadenceUnitValues.Month);
        deserialized.Interval.Should().Be(2);

        var apiJson = JsonSerializer.Serialize(
            new RecurringRuleCreateRequest { Cadence = cadence },
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );
        apiJson.Should().Contain("\"cadence\":{\"version\":1,\"unit\":\"Month\",\"interval\":2}");
        JsonSerializer
            .Deserialize<RecurringRuleCreateRequest>(
                apiJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            )!
            .Cadence.Unit.Should()
            .Be(RecurringCadenceUnitValues.Month);
    }

    [Fact]
    public void RecurringCadenceSerializer_ShouldRejectUnsupportedDefinitions()
    {
        Action[] invalidDefinitions =
        [
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 2,
                        Unit = RecurringCadenceUnitValues.Day,
                        Interval = 1,
                    }
                ),
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 1,
                        Unit = "Fortnight",
                        Interval = 1,
                    }
                ),
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 1,
                        Unit = RecurringCadenceUnitValues.Day,
                        Interval = 0,
                    }
                ),
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 1,
                        Unit = RecurringCadenceUnitValues.Week,
                        Interval = 8,
                        Mode = RecurringCadenceModeValues.PerUnit,
                    }
                ),
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 1,
                        Unit = RecurringCadenceUnitValues.Day,
                        Interval = 2,
                        Mode = RecurringCadenceModeValues.PerUnit,
                    }
                ),
            () =>
                RecurringCadenceSerializer.Validate(
                    new()
                    {
                        Version = 1,
                        Unit = RecurringCadenceUnitValues.Week,
                        Interval = 2,
                        Mode = "Unsupported",
                    }
                ),
            () => RecurringCadenceSerializer.Deserialize("{\"version\":1,\"unit\":\"Day\"}"),
            () => RecurringCadenceSerializer.Deserialize("not-json"),
        ];

        foreach (var invalidDefinition in invalidDefinitions)
        {
            invalidDefinition.Should().Throw<RecurringCadenceValidationException>();
        }
    }

    #endregion
    #region ReadForecastAsync
    [Fact]
    public async Task ReadForecastAsync_ShouldSuppressOnlyTheNearestOccurrence()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.Cadence = RecurringCadenceSerializer.Serialize(
            new RecurringCadence
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Day,
                Interval = 5,
            }
        );
        var transaction = AddTransaction(helper, account, new DateOnly(2026, 8, 4), 100);
        transaction.RecurringRule = rule;
        rule.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        var forecast = await CreateService(helper, new DateOnly(2026, 8, 1))
            .ReadForecastAsync(helper.demoUser.Id, new DateOnly(2026, 8, 1));

        forecast.Should().ContainSingle(item => item.Date == new DateOnly(2026, 8, 1));
    }

    [Fact]
    public async Task ReadForecastAsync_ShouldLeaveEquallyCloseOccurrencesUnpaired()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.Cadence = RecurringCadenceSerializer.Serialize(
            new RecurringCadence
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Day,
                Interval = 5,
            }
        );
        var firstTransaction = AddTransaction(helper, account, new DateOnly(2026, 8, 4), 100);
        var secondTransaction = AddTransaction(helper, account, new DateOnly(2026, 8, 8), 100);
        firstTransaction.RecurringRule = rule;
        secondTransaction.RecurringRule = rule;
        rule.Transactions.Add(firstTransaction);
        rule.Transactions.Add(secondTransaction);
        await helper.UserDataContext.SaveChangesAsync();

        var forecast = await CreateService(helper, new DateOnly(2026, 8, 1))
            .ReadForecastAsync(helper.demoUser.Id, new DateOnly(2026, 8, 1));

        forecast.Should().Contain(item => item.Date == new DateOnly(2026, 8, 1));
        forecast.Should().Contain(item => item.Date == new DateOnly(2026, 8, 6));
        forecast.Should().Contain(item => item.Date == new DateOnly(2026, 8, 11));
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
            Cadence = RecurringCadenceSerializer.Serialize(
                new()
                {
                    Version = 1,
                    Unit = RecurringCadenceUnitValues.Month,
                    Interval = 1,
                }
            ),
            StartDate = new DateOnly(2026, 1, 1),
            IsActive = true,
            AmountMode = RecurringAmountMode.Automatic,
            Amount = 100,
        };
        var historicalTransactions = new[] { 100m, 120m, 140m }.Select(
            (amount, index) =>
                new Transaction
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
    public async Task ReadForecastAsync_ShouldSuppressEachMatchedPerUnitOccurrence()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.Cadence = RecurringCadenceSerializer.Serialize(
            new RecurringCadence
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Month,
                Interval = 2,
                Mode = RecurringCadenceModeValues.PerUnit,
            }
        );
        rule.StartDate = new DateOnly(2026, 8, 1);
        var firstTransaction = AddTransaction(helper, account, new DateOnly(2026, 8, 1), 100);
        var secondTransaction = AddTransaction(helper, account, new DateOnly(2026, 8, 16), 100);
        firstTransaction.RecurringRule = rule;
        secondTransaction.RecurringRule = rule;
        rule.Transactions.Add(firstTransaction);
        rule.Transactions.Add(secondTransaction);
        await helper.UserDataContext.SaveChangesAsync();

        var forecast = await CreateService(helper, new DateOnly(2026, 8, 1))
            .ReadForecastAsync(helper.demoUser.Id, new DateOnly(2026, 8, 1));

        forecast.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadForecastAsync_ShouldUseEmptyAccountNameWhenAccountNameIsNull()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        account.Name = null!;
        AddRule(helper, account);
        await helper.UserDataContext.SaveChangesAsync();

        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var forecast = await service.ReadForecastAsync(
            helper.demoUser.Id,
            new DateOnly(2026, 8, 1)
        );

        forecast.Should().ContainSingle();
        forecast[0].AccountName.Should().BeEmpty();
    }

    #endregion
    #region MatchTransactionAsync
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
    public async Task MatchTransactionAsync_ShouldMatchASecondPerUnitOccurrence()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.Cadence = RecurringCadenceSerializer.Serialize(
            new RecurringCadence
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Month,
                Interval = 2,
                Mode = RecurringCadenceModeValues.PerUnit,
            }
        );
        rule.StartDate = new DateOnly(2026, 8, 1);
        var transaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 16),
            MerchantName = rule.MerchantName,
            Category = rule.Category,
            Subcategory = rule.Subcategory,
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        await CreateService(helper, new DateOnly(2026, 8, 1))
            .MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().Be(rule.ID);
    }

    [Fact]
    public async Task MatchTransactionAsync_ShouldMatchZeroAmountExactly()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account, amount: 0);
        var transaction = new Transaction
        {
            Amount = 0,
            Date = new DateOnly(2026, 8, 1),
            MerchantName = "Merchant",
            Category = "Category",
            Subcategory = "Subcategory",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().Be(rule.ID);
    }

    [Fact]
    public async Task MatchTransactionAsync_ShouldMatchTransactionsWithNullFields()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.MerchantName = null;
        rule.Category = null;
        rule.Subcategory = null;
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 1),
            MerchantName = null,
            Category = null,
            Subcategory = null,
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().Be(rule.ID);
    }

    [Fact]
    public async Task MatchTransactionAsync_ShouldRejectOccurrencesOutsideDateWindow()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        AddRule(helper, account);
        var transaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 10),
            MerchantName = "Merchant",
            Category = "Category",
            Subcategory = "Subcategory",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.MatchTransactionAsync(helper.demoUser.Id, transaction);

        transaction.RecurringRuleID.Should().BeNull();
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

    #endregion
    #region MatchingHelpers
    [Fact]
    public void IsTransactionMatch_ShouldIgnoreTheTransactionBeingMatched()
    {
        var rule = CreateRule(RecurringCadenceUnitValues.Month, 1, new DateOnly(2026, 8, 1));
        rule.MerchantName = "Merchant";
        rule.Category = "Category";
        rule.Subcategory = "Subcategory";
        rule.Amount = 100;
        var transaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 1),
            MerchantName = "Merchant",
            Category = "Category",
            Subcategory = "Subcategory",
            Source = TransactionSource.Manual,
            AccountID = rule.AccountID,
        };
        rule.Transactions.Add(transaction);

        var method = typeof(RecurringRuleService).GetMethod(
            "IsTransactionMatch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        method.Should().NotBeNull();
        method!.Invoke(null, [rule, transaction]).Should().Be(true);
    }

    [Fact]
    public void GetPairedOccurrences_ShouldInferRangeFromTransactionsWhenRangeIsOmitted()
    {
        var transactionDate = new DateOnly(2026, 8, 10);
        var rule = CreateRule(RecurringCadenceUnitValues.Day, 1, transactionDate);
        var transaction = new Transaction
        {
            Amount = 100,
            Date = transactionDate,
            Source = TransactionSource.Manual,
            AccountID = rule.AccountID,
        };
        rule.Transactions.Add(transaction);

        var method = typeof(RecurringRuleService).GetMethod(
            "GetPairedOccurrences",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        method.Should().NotBeNull();
        var pairs =
            (IReadOnlyDictionary<Transaction, DateOnly>)
                method!.Invoke(null, [rule, null, null, null])!;

        pairs.Should().ContainKey(transaction);
        pairs[transaction].Should().Be(transactionDate);
    }

    #endregion
    #region RecurringRuleCrud
    [Fact]
    public async Task CreateRecurringRuleAsync_ShouldRejectAMissingStartDate()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));
        var request = CreateRequest(account.ID);
        request.StartDate = DateOnly.MinValue;

        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, request),
            "RecurringRuleAccountAndStartDateRequiredError"
        );
    }

    [Fact]
    public async Task ReadRecurringRulesAsync_ShouldOnlyReturnRulesOwnedByUser()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        AddRule(helper, account);
        var otherUser = new ApplicationUser { UserName = "other-user" };
        helper.UserDataContext.Users.Add(otherUser);
        helper.UserDataContext.SaveChanges();
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var otherUserRules = await service.ReadRecurringRulesAsync(otherUser.Id);
        var userRules = await service.ReadRecurringRulesAsync(helper.demoUser.Id);

        otherUserRules.Should().BeEmpty();
        userRules.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateRecurringRuleAsync_ShouldCreateRuleWithAndWithoutTransaction()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.CreateRecurringRuleAsync(
            helper.demoUser.Id,
            CreateRequest(account.ID, amount: 42, endDate: new DateOnly(2026, 12, 31))
        );
        var rulesAfterCreate = await service.ReadRecurringRulesAsync(helper.demoUser.Id);
        rulesAfterCreate
            .Should()
            .ContainSingle(rule =>
                rule.AccountID == account.ID
                && rule.Amount == 42
                && rule.EndDate == new DateOnly(2026, 12, 31)
            );

        var transaction = AddTransaction(helper, account, new DateOnly(2026, 8, 1), 75);
        await service.CreateRecurringRuleAsync(
            helper.demoUser.Id,
            CreateRequest(account.ID, merchantName: "Attached", amount: 75),
            transaction.ID
        );

        var ruleWithTransaction = (
            await service.ReadRecurringRulesAsync(helper.demoUser.Id)
        ).Single(rule => rule.MerchantName == "Attached");
        ruleWithTransaction.MerchantName.Should().Be("Attached");
        ruleWithTransaction.MatchedTransactionCount.Should().Be(1);
        transaction.RecurringRuleID.Should().Be(ruleWithTransaction.ID);
    }

    [Fact]
    public async Task CreateRecurringRuleAsync_ShouldRejectInvalidReferencesAndRequests()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await AssertServiceException(
            () =>
                service.CreateRecurringRuleAsync(helper.demoUser.Id, CreateRequest(Guid.NewGuid())),
            "RecurringRuleAccountNotFoundError"
        );
        await AssertServiceException(
            () =>
                service.CreateRecurringRuleAsync(
                    helper.demoUser.Id,
                    CreateRequest(account.ID),
                    Guid.NewGuid()
                ),
            "TransactionNotFoundError"
        );

        var otherAccount = AddAccount(helper, "Savings");
        var transaction = AddTransaction(helper, otherAccount, new DateOnly(2026, 8, 1), 100);
        await AssertServiceException(
            () =>
                service.CreateRecurringRuleAsync(
                    helper.demoUser.Id,
                    CreateRequest(account.ID),
                    transaction.ID
                ),
            "RecurringRuleAccountMismatchError"
        );

        var assignedRule = AddRule(helper, account);
        transaction = AddTransaction(
            helper,
            account,
            new DateOnly(2026, 8, 1),
            100,
            assignedRule.ID
        );
        await AssertServiceException(
            () =>
                service.CreateRecurringRuleAsync(
                    helper.demoUser.Id,
                    CreateRequest(account.ID),
                    transaction.ID
                ),
            "TransactionAlreadyRecurringError"
        );

        var invalidCadence = CreateRequest(account.ID);
        invalidCadence.Cadence = new() { Unit = "Daily" };
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, invalidCadence),
            "RecurringRuleInvalidCadenceError"
        );

        var invalidAmountMode = CreateRequest(account.ID);
        invalidAmountMode.AmountMode = "Variable";
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, invalidAmountMode),
            "RecurringRuleInvalidAmountModeError"
        );

        var missingAccount = CreateRequest(Guid.Empty);
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, missingAccount),
            "RecurringRuleAccountAndStartDateRequiredError"
        );

        var missingStartDate = CreateRequest(account.ID);
        missingStartDate.StartDate = DateOnly.MinValue;
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, missingStartDate),
            "RecurringRuleAccountAndStartDateRequiredError"
        );

        var invalidEndDate = CreateRequest(account.ID);
        invalidEndDate.EndDate = invalidEndDate.StartDate.AddDays(-1);
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, invalidEndDate),
            "RecurringRuleEndDateBeforeStartDateError"
        );

        var zeroAmount = CreateRequest(account.ID);
        zeroAmount.Amount = 0;
        await AssertServiceException(
            () => service.CreateRecurringRuleAsync(helper.demoUser.Id, zeroAmount),
            "RecurringRuleZeroAmountError"
        );
    }

    [Fact]
    public async Task UpdateRecurringRuleAsync_ShouldUpdateRuleAndRejectInvalidReferences()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var replacementAccount = AddAccount(helper, "Savings");
        var rule = AddRule(helper, account);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var request = new RecurringRuleUpdateRequest
        {
            ID = rule.ID,
            AccountID = replacementAccount.ID,
            MerchantName = "Updated Merchant",
            Category = "Updated Category",
            Subcategory = "Updated Subcategory",
            Cadence = new()
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Week,
                Interval = 1,
            },
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 12, 31),
            IsActive = false,
            AmountMode = RecurringAmountModeValues.Automatic,
            Amount = 250,
        };

        await service.UpdateRecurringRuleAsync(helper.demoUser.Id, request);
        var response = (await service.ReadRecurringRulesAsync(helper.demoUser.Id)).Single();

        response.AccountID.Should().Be(replacementAccount.ID);
        response.MerchantName.Should().Be("Updated Merchant");
        response.Cadence.Unit.Should().Be(RecurringCadenceUnitValues.Week);
        response.Cadence.Interval.Should().Be(1);
        response.IsActive.Should().BeFalse();
        response.AmountMode.Should().Be(RecurringAmountModeValues.Automatic);
        response.Amount.Should().Be(250);

        request.ID = Guid.NewGuid();
        await AssertServiceException(
            () => service.UpdateRecurringRuleAsync(helper.demoUser.Id, request),
            "RecurringRuleNotFoundError"
        );

        request.ID = rule.ID;
        request.AccountID = Guid.NewGuid();
        await AssertServiceException(
            () => service.UpdateRecurringRuleAsync(helper.demoUser.Id, request),
            "RecurringRuleAccountNotFoundError"
        );
    }

    [Fact]
    public async Task DeleteRecurringRuleAsync_ShouldDeleteRuleAndRejectMissingRule()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await service.DeleteRecurringRuleAsync(helper.demoUser.Id, rule.ID);

        (await service.ReadRecurringRulesAsync(helper.demoUser.Id)).Should().BeEmpty();
        await AssertServiceException(
            () => service.DeleteRecurringRuleAsync(helper.demoUser.Id, rule.ID),
            "RecurringRuleNotFoundError"
        );
    }

    #endregion
    #region AdditionalForecastScenarios
    [Fact]
    public async Task ReadForecastAsync_ShouldFilterRulesAndCalculateAutomaticAmounts()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var hiddenAccount = AddAccount(helper, "Hidden", hideTransactions: true);
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var fixedRule = AddRule(helper, account, amount: 10);
        var evenRule = AddRule(helper, account, merchantName: "Even", amount: 999);
        evenRule.AmountMode = RecurringAmountMode.Automatic;
        evenRule.Transactions =
        [
            AddTransaction(helper, account, new DateOnly(2026, 6, 1), 40),
            AddTransaction(helper, account, new DateOnly(2026, 7, 1), 60),
        ];
        var oneTransactionRule = AddRule(helper, account, merchantName: "One", amount: 88);
        oneTransactionRule.AmountMode = RecurringAmountMode.Automatic;
        oneTransactionRule.Transactions.Add(
            AddTransaction(helper, account, new DateOnly(2026, 7, 1), 33)
        );
        var deletedTransactionsRule = AddRule(helper, account, merchantName: "Deleted", amount: 77);
        deletedTransactionsRule.AmountMode = RecurringAmountMode.Automatic;
        deletedTransactionsRule.Transactions =
        [
            AddTransaction(helper, account, new DateOnly(2026, 6, 1), 12, deleted: DateTime.UtcNow),
            AddTransaction(helper, account, new DateOnly(2026, 7, 1), 14, deleted: DateTime.UtcNow),
        ];
        var endingRule = AddRule(helper, account, merchantName: "Ending", amount: 25);
        endingRule.EndDate = new DateOnly(2026, 12, 31);

        AddRule(helper, account, category: "Transfer", amount: 20);
        AddRule(helper, account, category: TransactionCategoriesConstants.HideFromBudgetsCategory);
        AddRule(helper, hiddenAccount, merchantName: "Hidden account");
        var inactiveRule = AddRule(helper, account, merchantName: "Inactive");
        inactiveRule.IsActive = false;
        var futureRule = AddRule(helper, account, merchantName: "Future");
        futureRule.StartDate = new DateOnly(2026, 9, 1);
        var endedRule = AddRule(helper, account, merchantName: "Ended");
        endedRule.EndDate = new DateOnly(2026, 7, 31);
        await helper.UserDataContext.SaveChangesAsync();

        var forecast = await service.ReadForecastAsync(
            helper.demoUser.Id,
            new DateOnly(2026, 8, 1)
        );

        forecast.Should().HaveCount(5);
        forecast.Should().ContainSingle(item => item.RuleID == fixedRule.ID && item.Amount == 10);
        forecast.Should().ContainSingle(item => item.RuleID == evenRule.ID && item.Amount == 50);
        forecast.Should().ContainSingle(item => item.RuleID == endingRule.ID && item.Amount == 25);
        forecast
            .Should()
            .ContainSingle(item => item.RuleID == oneTransactionRule.ID && item.Amount == 88);
        forecast
            .Should()
            .ContainSingle(item => item.RuleID == deletedTransactionsRule.ID && item.Amount == 77);

        (await service.ReadForecastAsync(helper.demoUser.Id, new DateOnly(2026, 7, 1)))
            .Should()
            .BeEmpty();
        (await service.ReadForecastAsync(helper.demoUser.Id, new DateOnly(2026, 9, 1)))
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task ReadForecastAsync_ShouldKeepOccurrenceWhenNearestTransactionsAreAmbiguous()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var rule = AddRule(helper, account);
        rule.StartDate = new DateOnly(2026, 8, 10);
        rule.Transactions =
        [
            AddTransaction(helper, account, new DateOnly(2026, 8, 8), 100),
            AddTransaction(helper, account, new DateOnly(2026, 8, 12), 100),
        ];
        await helper.UserDataContext.SaveChangesAsync();
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var forecast = await service.ReadForecastAsync(
            helper.demoUser.Id,
            new DateOnly(2026, 8, 1)
        );

        forecast
            .Should()
            .ContainSingle(item =>
                item.RuleID == rule.ID && item.Date == new DateOnly(2026, 8, 10)
            );
    }

    #endregion
    #region AdditionalTransactionMatching
    [Fact]
    public async Task MatchTransactionAsync_ShouldSkipExcludedTransactionsAndUnmatchedRules()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var inactiveRule = AddRule(helper, account, merchantName: "Inactive");
        inactiveRule.IsActive = false;
        var otherAccount = AddAccount(helper, "Other");
        AddRule(helper, otherAccount, merchantName: "Other account");
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        var skippedTransactions = new[]
        {
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                RecurringRuleID = Guid.NewGuid(),
            },
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                Deleted = DateTime.UtcNow,
            },
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                Category = "Transfer",
            },
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                Category = TransactionCategoriesConstants.HideFromBudgetsCategory,
            },
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                SourceTransactionLink = new TransactionLink
                {
                    SourceTransactionID = Guid.NewGuid(),
                    TargetTransactionID = Guid.NewGuid(),
                },
            },
            new Transaction
            {
                Amount = 100,
                Date = new DateOnly(2026, 8, 1),
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
                TargetTransactionLink = new TransactionLink
                {
                    SourceTransactionID = Guid.NewGuid(),
                    TargetTransactionID = Guid.NewGuid(),
                },
            },
        };

        foreach (var transaction in skippedTransactions)
        {
            var initialRuleID = transaction.RecurringRuleID;
            await service.MatchTransactionAsync(helper.demoUser.Id, transaction);
            transaction.RecurringRuleID.Should().Be(initialRuleID);
        }

        var unmatchedRule = AddRule(helper, account, merchantName: "Expected");
        var unmatchedTransaction = new Transaction
        {
            Amount = 100,
            Date = new DateOnly(2026, 8, 1),
            MerchantName = "Different",
            Category = unmatchedRule.Category,
            Subcategory = unmatchedRule.Subcategory,
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        await service.MatchTransactionAsync(helper.demoUser.Id, unmatchedTransaction);
        unmatchedTransaction.RecurringRuleID.Should().BeNull();
    }

    #endregion
    #region AssignAndUnassignTransactionAsync
    [Fact]
    public async Task AssignAndUnassignTransactionAsync_ShouldManageAssignmentsAndRejectInvalidReferences()
    {
        var helper = new TestHelper();
        var account = AddAccount(helper);
        var otherAccount = AddAccount(helper, "Savings");
        var rule = AddRule(helper, account);
        var otherRule = AddRule(helper, account, merchantName: "Other");
        var service = CreateService(helper, new DateOnly(2026, 8, 1));

        await AssertServiceException(
            () =>
                service.AssignTransactionAsync(helper.demoUser.Id, Guid.NewGuid(), Guid.NewGuid()),
            "RecurringRuleNotFoundError"
        );

        var transaction = AddTransaction(helper, account, new DateOnly(2026, 8, 1), 100);
        await service.AssignTransactionAsync(helper.demoUser.Id, rule.ID, transaction.ID);
        transaction.RecurringRuleID.Should().Be(rule.ID);

        await service.AssignTransactionAsync(helper.demoUser.Id, rule.ID, transaction.ID);
        transaction.RecurringRuleID.Should().Be(rule.ID);

        await AssertServiceException(
            () => service.AssignTransactionAsync(helper.demoUser.Id, otherRule.ID, transaction.ID),
            "TransactionAlreadyRecurringError"
        );

        var otherAccountTransaction = AddTransaction(
            helper,
            otherAccount,
            new DateOnly(2026, 8, 1),
            100
        );
        await AssertServiceException(
            () =>
                service.AssignTransactionAsync(
                    helper.demoUser.Id,
                    rule.ID,
                    otherAccountTransaction.ID
                ),
            "RecurringRuleAccountMismatchError"
        );
        await AssertServiceException(
            () => service.AssignTransactionAsync(helper.demoUser.Id, rule.ID, Guid.NewGuid()),
            "TransactionNotFoundError"
        );

        await service.UnassignTransactionAsync(helper.demoUser.Id, transaction.ID);
        transaction.RecurringRuleID.Should().BeNull();
        await AssertServiceException(
            () => service.UnassignTransactionAsync(helper.demoUser.Id, Guid.NewGuid()),
            "TransactionNotFoundError"
        );
    }

    #endregion

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

    private static RecurringRuleCreateRequest CreateRequest(
        Guid accountID,
        string merchantName = "Merchant",
        decimal amount = 100,
        DateOnly? endDate = null
    ) =>
        new()
        {
            AccountID = accountID,
            MerchantName = merchantName,
            Category = "Category",
            Subcategory = "Subcategory",
            Cadence = new()
            {
                Version = 1,
                Unit = RecurringCadenceUnitValues.Month,
                Interval = 1,
            },
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = endDate,
            IsActive = true,
            AmountMode = RecurringAmountModeValues.Fixed,
            Amount = amount,
        };

    private static async Task AssertServiceException(Func<Task> action, string message)
    {
        var exception = await Assert.ThrowsAsync<BudgetBoardServiceException>(action);
        exception.Message.Should().Be(message);
    }

    private static Account AddAccount(
        TestHelper helper,
        string name = "Checking",
        bool hideTransactions = false
    )
    {
        var account = new Account
        {
            Name = name,
            InstitutionID = Guid.NewGuid(),
            Type = "checking",
            HideTransactions = hideTransactions,
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();
        return account;
    }

    private static Transaction AddTransaction(
        TestHelper helper,
        Account account,
        DateOnly date,
        decimal amount,
        Guid? recurringRuleID = null,
        DateTime? deleted = null
    )
    {
        var transaction = new Transaction
        {
            Amount = amount,
            Date = date,
            MerchantName = "Merchant",
            Category = "Category",
            Subcategory = "Subcategory",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
            RecurringRuleID = recurringRuleID,
            Deleted = deleted,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();
        return transaction;
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
            Cadence = RecurringCadenceSerializer.Serialize(
                new()
                {
                    Version = 1,
                    Unit = RecurringCadenceUnitValues.Month,
                    Interval = 1,
                }
            ),
            StartDate = new DateOnly(2026, 8, 1),
            IsActive = true,
            AmountMode = RecurringAmountMode.Fixed,
            Amount = amount,
        };
        helper.UserDataContext.RecurringRules.Add(rule);
        helper.UserDataContext.SaveChanges();
        return rule;
    }

    private static RecurringRule CreateRule(
        string unit,
        int interval,
        DateOnly startDate,
        string? mode = null
    ) =>
        new()
        {
            UserID = Guid.NewGuid(),
            AccountID = Guid.NewGuid(),
            Cadence = RecurringCadenceSerializer.Serialize(
                new RecurringCadence
                {
                    Version = 1,
                    Unit = unit,
                    Interval = interval,
                    Mode = mode,
                }
            ),
            StartDate = startDate,
            IsActive = true,
            Amount = 100,
        };
}
