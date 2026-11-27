using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
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
public class NotificationActivityServiceTests
{
    [Fact]
    public async Task CompleteAsync_WhenBudgetAndLargeTransactionThresholdsAreCrossed_DispatchesOneSummaryPerType()
    {
        var helper = new TestHelper();
        var today = new DateOnly(2026, 11, 12);
        var settings = new UserSettings
        {
            UserID = helper.demoUser.Id,
            BudgetWarningThreshold = 80,
            LargeTransactionThreshold = 40,
        };
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.UserSettings.Add(settings);
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var budgetService = new Mock<IBudgetService>();
        budgetService
            .Setup(service => service.ReadBudgetsAsync(helper.demoUser.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(
                (IReadOnlyList<IBudgetResponse>)
                [
                    new BudgetResponse(
                        new Budget
                        {
                            ID = Guid.NewGuid(),
                            Month = new DateOnly(today.Year, today.Month, 1),
                            Category = "Food",
                            Limit = 100,
                            UserID = helper.demoUser.Id,
                        }
                    ),
                ]
            );
        var categoryService = new Mock<ITransactionCategoryService>();
        categoryService
            .Setup(service => service.ReadTransactionCategoriesAsync(helper.demoUser.Id))
            .ReturnsAsync(
                (IReadOnlyList<ITransactionCategoryResponse>)
                [
                    new CategoryResponse(
                        new TransactionCategoryBase
                        {
                            Value = "Food",
                            Parent = string.Empty,
                            CategoryType = TransactionCategoryTypes.Expense,
                        }
                    ),
                ]
            );
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service => service.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);
        var service = new NotificationActivityService(
            helper.UserDataContext,
            budgetService.Object,
            categoryService.Object,
            dispatcher.Object,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>(),
            Mock.Of<ILogger<NotificationActivityService>>()
        );

        await using var scope = await service.BeginAsync(helper.demoUser.Id);
        foreach (var amount in new[] { -45m, -40m })
        {
            var transaction = new Transaction
            {
                Amount = amount,
                Date = today,
                Category = "Food",
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
            };
            helper.UserDataContext.Transactions.Add(transaction);
            await helper.UserDataContext.SaveChangesAsync();
            await service.TrackTransactionCreatedAsync(helper.demoUser.Id, transaction);
        }

        await scope.CompleteAsync();

        dispatcher.Verify(
            item => item.DispatchAsync(
                It.Is<NotificationEvent>(notification =>
                    notification.Type == NotificationEventTypes.BudgetWarning
                )
            ),
            Times.Once
        );
        dispatcher.Verify(
            item => item.DispatchAsync(
                It.Is<NotificationEvent>(notification =>
                    notification.Type == NotificationEventTypes.TransactionsAdded
                )
            ),
            Times.Once
        );
        dispatcher.Verify(
            item => item.DispatchAsync(
                It.Is<NotificationEvent>(notification =>
                    notification.Type == NotificationEventTypes.LargeTransactionsAdded
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task CompleteAsync_WhenOnlyTransfersWereAdded_DoesNotDispatchTransactionActivity()
    {
        var helper = new TestHelper();
        var today = new DateOnly(2026, 11, 12);
        var settings = new UserSettings
        {
            UserID = helper.demoUser.Id,
            LargeTransactionThreshold = 1,
        };
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.UserSettings.Add(settings);
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var budgetService = new Mock<IBudgetService>();
        budgetService
            .Setup(service => service.ReadBudgetsAsync(helper.demoUser.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync((IReadOnlyList<IBudgetResponse>)[]);
        var categoryService = new Mock<ITransactionCategoryService>();
        categoryService
            .Setup(service => service.ReadTransactionCategoriesAsync(helper.demoUser.Id))
            .ReturnsAsync((IReadOnlyList<ITransactionCategoryResponse>)[]);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service => service.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);
        var service = new NotificationActivityService(
            helper.UserDataContext,
            budgetService.Object,
            categoryService.Object,
            dispatcher.Object,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>(),
            Mock.Of<ILogger<NotificationActivityService>>()
        );

        await using var scope = await service.BeginAsync(helper.demoUser.Id);
        var transaction = new Transaction
        {
            Amount = -500,
            Date = today,
            Category = TransactionCategoriesConstants.TransferCategory,
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();
        await service.TrackTransactionCreatedAsync(helper.demoUser.Id, transaction);
        await scope.CompleteAsync();

        dispatcher.Verify(
            item => item.DispatchAsync(
                It.Is<NotificationEvent>(notification =>
                    notification.Type == NotificationEventTypes.TransactionsAdded
                    || notification.Type == NotificationEventTypes.LargeTransactionsAdded
                )
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task CompleteAsync_WhenBackdatedTransactionCrossesBudgetThreshold_DispatchesHistoricalMonthWarning()
    {
        var helper = new TestHelper();
        var today = new DateOnly(2026, 11, 12);
        var transactionDate = new DateOnly(2026, 10, 12);
        var settings = new UserSettings
        {
            UserID = helper.demoUser.Id,
            BudgetWarningThreshold = 80,
        };
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.UserSettings.Add(settings);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(
            new Transaction
            {
                Amount = -50,
                Date = transactionDate,
                Category = "Food",
                Source = TransactionSource.Manual,
                AccountID = account.ID,
                Account = account,
            }
        );
        await helper.UserDataContext.SaveChangesAsync();

        var budgetService = new Mock<IBudgetService>();
        budgetService
            .Setup(service => service.ReadBudgetsAsync(helper.demoUser.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(
                (IReadOnlyList<IBudgetResponse>)
                [
                    new BudgetResponse(
                        new Budget
                        {
                            ID = Guid.NewGuid(),
                            Month = new DateOnly(2026, 10, 1),
                            Category = "Food",
                            Limit = 100,
                            UserID = helper.demoUser.Id,
                        }
                    ),
                ]
            );
        var categoryService = new Mock<ITransactionCategoryService>();
        categoryService
            .Setup(service => service.ReadTransactionCategoriesAsync(helper.demoUser.Id))
            .ReturnsAsync(
                (IReadOnlyList<ITransactionCategoryResponse>)
                [
                    new CategoryResponse(
                        new TransactionCategoryBase
                        {
                            Value = "Food",
                            Parent = string.Empty,
                            CategoryType = TransactionCategoryTypes.Expense,
                        }
                    ),
                ]
            );
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service => service.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);
        var service = new NotificationActivityService(
            helper.UserDataContext,
            budgetService.Object,
            categoryService.Object,
            dispatcher.Object,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>(),
            Mock.Of<ILogger<NotificationActivityService>>()
        );

        await using var scope = await service.BeginAsync(helper.demoUser.Id);
        await service.TrackBudgetMonthAsync(helper.demoUser.Id, transactionDate);
        var transaction = new Transaction
        {
            Amount = -40,
            Date = transactionDate,
            Category = "Food",
            Source = TransactionSource.Manual,
            AccountID = account.ID,
            Account = account,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();
        await service.TrackTransactionCreatedAsync(helper.demoUser.Id, transaction);
        await scope.CompleteAsync();

        dispatcher.Verify(
            item => item.DispatchAsync(
                It.Is<NotificationEvent>(notification =>
                    notification.Type == NotificationEventTypes.BudgetWarning
                )
            ),
            Times.Once
        );
    }
}
