using Bogus;
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
public class TransactionServiceTests
{
    #region CreateTransactionAsync
    [Fact]
    public async Task CreateTransactionAsync_WhenValidData_ShouldCreateTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(new Faker().Date.Past()),
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            Source = "manual",
            AccountID = account.ID,
        };

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser, transaction);

        // Assert
        var createdTransaction = helper
            .demoUser.Accounts.SelectMany(a => a.Transactions)
            .FirstOrDefault(t => t.MerchantName == transaction.MerchantName);
        createdTransaction.Should().NotBeNull();
        createdTransaction
            .Should()
            .BeEquivalentTo(transaction, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenAccountDoesNotExist_ShouldThrowTransactionAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(new Faker().Date.Past()),
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            AccountID = Guid.NewGuid(),
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.CreateTransactionAsync(helper.demoUser, transaction);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionAccountNotFoundError");
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenNewTransactionAndManualAccount_ShouldUpdateBalance()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(new Faker().Date.Past()),
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            AccountID = account.ID,
        };

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);
        // Assert
        helper.UserDataContext.Balances.Should().ContainSingle();
        helper.UserDataContext.Balances.Single().Date.Should().Be(transaction.Date);
        helper.UserDataContext.Balances.Single().Amount.Should().Be(transaction.Amount);
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenNewTransactionIsNotLatest_ShouldUpdateAllSubsequentBalances()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[4].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(fakeDate.AddDays(-2)),
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            AccountID = account.ID,
        };

        var oldBalance = balances[4].Amount;

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(6);
        helper.UserDataContext.Balances.ToList().ElementAt(4).Should().NotBeNull();
        helper.UserDataContext.Balances.ToList().ElementAt(4).Date.Should().Be(balances[4].Date);
        helper
            .UserDataContext.Balances.ToList()
            .ElementAt(4)
            .Amount.Should()
            .Be(oldBalance + transaction.Amount);
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenBalanceExistsForTransactionDate_ShouldUpdateExistingBalance()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);
        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[4].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = balances[2].Date,
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            AccountID = account.ID,
        };

        var oldBalanceOnTransactionDate = balances[2].Amount;
        var oldCurrentBalance = balances[4].Amount;

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser, transaction);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);

        helper.UserDataContext.Balances.ToList().ElementAt(2).Should().NotBeNull();
        helper.UserDataContext.Balances.ToList().ElementAt(2).Date.Should().Be(balances[2].Date);
        helper
            .UserDataContext.Balances.ToList()
            .ElementAt(2)
            .Amount.Should()
            .Be(oldBalanceOnTransactionDate + transaction.Amount);

        helper.UserDataContext.Balances.ToList().ElementAt(4).Should().NotBeNull();
        helper.UserDataContext.Balances.ToList().ElementAt(4).Date.Should().Be(balances[4].Date);
        helper
            .UserDataContext.Balances.ToList()
            .ElementAt(4)
            .Amount.Should()
            .Be(oldCurrentBalance + transaction.Amount);
    }
    #endregion

    #region ReadTransactionsAsync
    [Fact]
    public async Task ReadTransactionsAsync_WhenValidData_ShouldReturnTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            false
        );

        // Assert
        result.Should().HaveCount(5);
        result.Should().BeEquivalentTo(transactions.Select(t => new TransactionResponse(t)));
    }

    [Fact]
    public async Task ReadTransactionsAsync_WhenYearIsProvided_ShouldReturnTransactionsForYear()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            fakeDate.Year,
            null,
            false
        );

        // Assert
        result.Should().HaveCount(transactions.Count(t => t.Date.Year == fakeDate.Year));
        result
            .Should()
            .BeEquivalentTo(
                transactions
                    .Where(t => t.Date.Year == fakeDate.Year)
                    .Select(t => new TransactionResponse(t))
            );
    }

    [Fact]
    public async Task ReadTransactionsAsync_WhenMonthIsProvided_ShouldReturnTransactionsForMonth()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            fakeDate.Month,
            false
        );

        // Assert
        result.Should().HaveCount(transactions.Count(t => t.Date.Month == fakeDate.Month));
        result
            .Should()
            .BeEquivalentTo(
                transactions
                    .Where(t => t.Date.Month == fakeDate.Month)
                    .Select(t => new TransactionResponse(t))
            );
    }

    [Fact]
    public async Task ReadTransactionsAsync_WhenGetHiddenIsTrue_ShouldReturnHiddenTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            true
        );

        // Assert
        result.Should().HaveCount(5);
        result.Should().BeEquivalentTo(transactions.Select(t => new TransactionResponse(t)));
    }

    [Fact]
    public async Task ReadTransactionsAsync_WhenTransactionIsHidden_ShouldNotReturnHiddenTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;
        account.HideTransactions = true;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            false
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadTransactionsAsync_WhenAccountHasHideTransactions_ShouldNotReturnThoseTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.HideTransactions = true;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            false
        );

        // Assert
        result.Should().BeEmpty();
    }
    #endregion

    #region UpdateTransactionAsync
    [Fact]
    public async Task UpdateTransactionsAsync_WhenValidData_ShouldUpdateTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = transactions.First().ID,
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(new Faker().Date.Past()),
            Category = "newCategory",
            Subcategory = "newSubcategory",
            MerchantName = "newMerchantName",
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editedTransaction]);

        // Assert
        var updatedTransaction = helper
            .demoUser.Accounts.SelectMany(a => a.Transactions)
            .First(t => t.ID == editedTransaction.ID);
        updatedTransaction.AccountID.Should().Be(transactions.First().AccountID);
        updatedTransaction.Amount.Should().Be(editedTransaction.Amount);
        updatedTransaction.Date.Should().Be(editedTransaction.Date);
        updatedTransaction.Category.Should().Be(editedTransaction.Category.Value);
        updatedTransaction.Subcategory.Should().Be(editedTransaction.Subcategory.Value);
        updatedTransaction.MerchantName.Should().Be(editedTransaction.MerchantName.Value);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = Guid.NewGuid(),
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(new Faker().Date.Past()),
            Category = "newCategory",
            Subcategory = "newSubcategory",
            MerchantName = "newMerchantName",
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.UpdateTransactionsAsync(
                helper.demoUser.Id,
                [editedTransaction]
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenAmountUpdated_ShouldUpdateBalancesOnAndAfterThatDate()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[0].Amount = 100.0M;
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[1].Amount = 150.0M;
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[2].Amount = 150.0M;
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[3].Amount = 200.0M;
        balances[4].Date = DateOnly.FromDateTime(fakeDate);
        balances[4].Amount = 200.0M;

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        transactions.First().Date = balances[1].Date;
        transactions.First().Amount = 50.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = transactions.First().ID,
            Amount = 100.0M,
            Date = transactions.First().Date,
        };

        var oldBalanceDates = balances.Select(b => b.Date).ToList();
        var oldBalanceAmounts = balances.Select(b => b.Amount).ToList();

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editedTransaction]);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        for (int index = 0; index < oldBalanceDates.Count; index++)
        {
            var date = oldBalanceDates[index];
            var balance = helper.UserDataContext.Balances.Single(b => b.Date == date);
            if (date < editedTransaction.Date)
            {
                balance.Amount.Should().Be(oldBalanceAmounts[index]);
            }
            else
            {
                balance.Amount.Should().Be(oldBalanceAmounts[index] + 50.0M);
            }
        }
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenDateUpdated_ShouldMoveBalanceImpactToNewDate()
    {
        // Arrange
        var fakeDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        account.Balances =
        [
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 1),
                Amount = 100m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 2),
                Amount = 150m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 3),
                Amount = 150m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 4),
                Amount = 150m,
            },
        ];

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();
        transaction.Date = new DateOnly(2025, 1, 2);
        transaction.Amount = 50m;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequest = new TransactionUpdateRequest
        {
            ID = transaction.ID,
            Amount = 50m,
            Date = new DateOnly(2025, 1, 4),
            Category = transaction.Category,
            Subcategory = transaction.Subcategory,
            MerchantName = transaction.MerchantName,
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editRequest]);

        // Assert
        var balances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        balances[0].Amount.Should().Be(100m);
        balances[1].Amount.Should().Be(100m);
        balances[2].Amount.Should().Be(100m);
        balances[3].Amount.Should().Be(150m);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenDateAndAmountUpdated_ShouldMoveUpdatedBalanceImpact()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        account.Source = AccountSource.Manual;
        account.Balances =
        [
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 1),
                Amount = 100m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 2),
                Amount = 150m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 3),
                Amount = 150m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 4),
                Amount = 150m,
            },
        ];

        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Date = new DateOnly(2025, 1, 2);
        transaction.Amount = 50m;
        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequest = new TransactionUpdateRequest
        {
            ID = transaction.ID,
            Amount = 75m,
            Date = new DateOnly(2025, 1, 4),
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editRequest]);

        // Assert
        var balances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        balances.Select(b => b.Amount).Should().Equal(100m, 100m, 100m, 175m);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenMovedToDateWithoutBalance_ShouldCreateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        account.Source = AccountSource.Manual;
        account.Balances =
        [
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 1),
                Amount = 100m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 3),
                Amount = 150m,
            },
        ];

        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Date = new DateOnly(2025, 1, 3);
        transaction.Amount = 50m;
        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequest = new TransactionUpdateRequest
        {
            ID = transaction.ID,
            Date = new DateOnly(2025, 1, 2),
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editRequest]);

        // Assert
        var balances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        balances
            .Select(b => b.Date)
            .Should()
            .Equal(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 2), new DateOnly(2025, 1, 3));
        balances.Select(b => b.Amount).Should().Equal(100m, 150m, 150m);
    }
    #endregion

    [Fact]
    public async Task DeleteTransactionAsync_ShouldDeleteTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToDelete = transactions.First();

        // Act
        await transactionService.DeleteTransactionAsync(helper.demoUser.Id, transactionToDelete.ID);

        // Assert
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transactionToDelete.ID)
            .Deleted.Should()
            .NotBeNull();
    }

    [Fact]
    public async Task DeleteTransactionAsync_WhenTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.DeleteTransactionAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteTransactionAsync_WhenDeleteTransaction_ShouldUpdateBalance()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[4].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);
        transactions[0].Date = balances[0].Date;
        transactions[0].Amount = 50.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToDelete = transactions.First();

        var oldBalance = balances.Last().Amount;
        // Act
        await transactionService.DeleteTransactionAsync(helper.demoUser.Id, transactionToDelete.ID);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        helper.UserDataContext.Balances.ToList().Last().Should().NotBeNull();
        helper.UserDataContext.Balances.ToList().Last().Date.Should().Be(balances.Last().Date);
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldBalance - transactionToDelete.Amount);
    }

    [Fact]
    public async Task RestoreTransactionAsync_WhenTransactionIsDeleted_ShouldRestoreTransaction()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToRestore = transactions.First();
        transactionToRestore.Deleted = fakeDate;

        // Act
        await transactionService.RestoreTransactionAsync(
            helper.demoUser.Id,
            transactionToRestore.ID
        );

        // Assert
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transactionToRestore.ID)
            .Deleted.Should()
            .BeNull();
    }

    [Fact]
    public async Task RestoreTransactionAsync_WhenTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.RestoreTransactionAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionRestoreNotFoundError");
    }

    [Fact]
    public async Task SplitTransactionAsync_WhenSplitTransaction_ShouldSplitTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        transactions.First().Amount = 100.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToSplit = transactions.First();
        var transactionToSplitAmount = transactionToSplit.Amount;
        var splitTransactionRequest = new TransactionSplitRequest
        {
            ID = transactionToSplit.ID,
            Amount = 20.0M,
            Category = "test",
            Subcategory = "test2",
        };

        // Act
        await transactionService.SplitTransactionAsync(helper.demoUser.Id, splitTransactionRequest);

        // Assert
        helper.UserDataContext.Transactions.Should().HaveCount(6);
        helper.UserDataContext.Transactions.Last().Should().NotBeNull();
        helper.UserDataContext.Transactions.Last().ID.Should().NotBe(transactionToSplit.ID);
        helper
            .UserDataContext.Transactions.Last()
            .Amount.Should()
            .Be(splitTransactionRequest.Amount);
        helper
            .UserDataContext.Transactions.Last()
            .Category.Should()
            .Be(splitTransactionRequest.Category);
        helper
            .UserDataContext.Transactions.Last()
            .Subcategory.Should()
            .Be(splitTransactionRequest.Subcategory);
        helper
            .UserDataContext.Transactions.First()
            .Amount.Should()
            .Be(transactionToSplitAmount - splitTransactionRequest.Amount);
    }

    [Fact]
    public async Task SplitTransactionAsync_WhenTransactionDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var splitTransactionRequest = new TransactionSplitRequest
        {
            ID = Guid.NewGuid(),
            Amount = 20.0M,
            Category = "test",
            Subcategory = "test2",
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.SplitTransactionAsync(
                helper.demoUser.Id,
                splitTransactionRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionSplitNotFoundError");
    }

    [Theory]
    [InlineData(100.0, 200.0)]
    [InlineData(-100.0, -150.0)]
    public async Task SplitTransactionAsync_WhenSplitAmountTooLarge_ShouldThrowError(
        decimal originalAmount,
        decimal splitAmount
    )
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        transactions.First().Amount = originalAmount;
        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToSplit = transactions.First();
        var splitTransactionRequest = new TransactionSplitRequest
        {
            ID = transactionToSplit.ID,
            Amount = splitAmount,
            Category = "test",
            Subcategory = "test2",
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.SplitTransactionAsync(
                helper.demoUser.Id,
                splitTransactionRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionSplitInvalidAmountError");
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenValidData_ShouldImportTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var importRequest = new TransactionImportRequest
        {
            Transactions = transactions.Select(t => new TransactionImport
            {
                Date = t.Date,
                MerchantName = t.MerchantName ?? string.Empty,
                Category = t.Category,
                Amount = t.Amount,
                Account = "bongus",
            }),
            AccountNameToIDMap = [new() { AccountName = "bongus", AccountID = account.ID }],
        };

        // Act
        await transactionService.ImportTransactionsAsync(helper.demoUser.Id, importRequest);

        // Assert
        helper.UserDataContext.Transactions.Should().HaveCount(5);
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenAccountNotFound_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var importRequest = new TransactionImportRequest
        {
            Transactions = transactions.Select(t => new TransactionImport
            {
                Date = t.Date,
                MerchantName = t.MerchantName ?? string.Empty,
                Category = t.Category,
                Amount = t.Amount,
                Account = "bongus",
            }),
            AccountNameToIDMap = [],
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.ImportTransactionsAsync(helper.demoUser.Id, importRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionImportAccountNotFoundError");
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenAutoCategorizerProbabilityBelowThreshold_ShouldNotApplyCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        // Set up user settings with a very high threshold (99%) to ensure prediction is below
        helper.demoUser.UserSettings = new Database.Models.UserSettings
        {
            UserID = helper.demoUser.Id,
            EnableAutoCategorizer = true,
            AutoCategorizerMinimumProbabilityPercentage = 99,
        };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);

        // Train model with transactions
        var trainingTransactions = new List<Database.Models.Transaction>
        {
            new()
            {
                Amount = 50M,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                AccountID = account.ID,
                MerchantName = "Coffee Shop",
                Category = "Dining",
                Source = "test",
            },
            new()
            {
                Amount = 25M,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                AccountID = account.ID,
                MerchantName = "Gas Station",
                Category = "Transportation",
                Source = "test",
            },
        };
        helper.UserDataContext.Transactions.AddRange(trainingTransactions);
        helper.UserDataContext.SaveChanges();

        var mlModel = AutomaticTransactionCategorizerHelper.Train(trainingTransactions);

        var allCategories = new List<ITransactionCategory>
        {
            new TransactionCategoryBase { Value = "Dining", Parent = string.Empty },
            new TransactionCategoryBase { Value = "Transportation", Parent = string.Empty },
        };

        var autoCategorizer = new AutomaticTransactionCategorizerHelper(mlModel);

        // Create a transaction with a merchant name that might have lower confidence
        var transaction = new TransactionCreateRequest
        {
            SyncID = string.Empty,
            Amount = 15M,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MerchantName = "Random Store",
            Source = "test",
            AccountID = account.ID,
        };

        // Act
        await transactionService.CreateTransactionAsync(
            helper.demoUser,
            transaction,
            allCategories,
            autoCategorizer
        );

        // Assert
        var createdTransaction = helper.UserDataContext.Transactions.OrderBy(t => t.Date).Last();
        createdTransaction.Category.Should().BeNullOrEmpty();
        createdTransaction.Subcategory.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenAutoCategorizerProbabilityAboveThreshold_ShouldApplyCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        // Set up user settings with default threshold (70%)
        helper.demoUser.UserSettings = new Database.Models.UserSettings
        {
            UserID = helper.demoUser.Id,
            EnableAutoCategorizer = true,
            AutoCategorizerMinimumProbabilityPercentage = 70,
        };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);

        // Train model with transactions
        var trainingTransactions = new List<Database.Models.Transaction>
        {
            new()
            {
                Amount = 50M,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                AccountID = account.ID,
                MerchantName = "Coffee Shop",
                Category = "Dining",
                Source = "test",
            },
            new()
            {
                Amount = 55M,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                AccountID = account.ID,
                MerchantName = "Coffee Place",
                Category = "Dining",
                Source = "test",
            },
            new()
            {
                Amount = 25M,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Account = account,
                AccountID = account.ID,
                MerchantName = "Gas Station",
                Category = "Transportation",
                Source = "test",
            },
        };
        helper.UserDataContext.Transactions.AddRange(trainingTransactions);
        helper.UserDataContext.SaveChanges();

        var mlModel = AutomaticTransactionCategorizerHelper.Train(trainingTransactions);

        var allCategories = new List<ITransactionCategory>
        {
            new TransactionCategoryBase { Value = "Dining", Parent = string.Empty },
            new TransactionCategoryBase { Value = "Transportation", Parent = string.Empty },
        };

        var autoCategorizer = new AutomaticTransactionCategorizerHelper(mlModel);

        // Create a transaction with a merchant name similar to training data
        var transaction = new TransactionCreateRequest
        {
            SyncID = string.Empty,
            Amount = 52M,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MerchantName = "Coffee Shop",
            Source = "test",
            AccountID = account.ID,
        };

        // Act
        await transactionService.CreateTransactionAsync(
            helper.demoUser,
            transaction,
            allCategories,
            autoCategorizer
        );

        // Assert
        var createdTransaction = helper.UserDataContext.Transactions.OrderBy(t => t.Date).Last();
        createdTransaction.Category.Should().Be("Dining");
    }

    [Fact]
    public async Task UpdateTransactionsAsync_ShouldUpdateAllTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(3);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequests = transactions
            .Select(t => new TransactionUpdateRequest
            {
                ID = t.ID,
                Amount = 99.0M,
                Date = DateOnly.FromDateTime(new Faker().Date.Past()),
                Category = "batchCategory",
                Subcategory = "batchSubcategory",
                MerchantName = "batchMerchant",
            })
            .ToList();

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, editRequests);

        // Assert
        // foreach (var req in editRequests)
        // {
        //     helper
        //         .UserDataContext.Transactions.Single(t => t.ID == req.ID)
        //         .Should()
        //         .BeEquivalentTo(req);
        // }
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenAnyTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequests = new List<TransactionUpdateRequest>
        {
            new()
            {
                ID = transactions.First().ID,
                Amount = 50.0M,
                Date = DateOnly.FromDateTime(new Faker().Date.Past()),
                Category = "cat",
                Subcategory = "sub",
                MerchantName = "merchant",
            },
            new()
            {
                ID = Guid.NewGuid(), // does not exist
                Amount = 50.0M,
                Date = DateOnly.FromDateTime(new Faker().Date.Past()),
                Category = "cat",
                Subcategory = "sub",
                MerchantName = "merchant",
            },
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, editRequests);

        // Assert
        // await act.Should()
        //     .ThrowAsync<BudgetBoardServiceException>()
        //     .WithMessage("TransactionNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenDuplicateIdsInRequest_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var duplicateId = transaction.ID;
        var editRequests = new List<TransactionUpdateRequest>
        {
            new()
            {
                ID = duplicateId,
                Amount = 50.0M,
                Date = DateOnly.FromDateTime(new Faker().Date.Past()),
                Category = "cat",
                Subcategory = "sub",
                MerchantName = "merchant",
            },
            new()
            {
                ID = duplicateId, // same ID as above
                Amount = 75.0M,
                Date = DateOnly.FromDateTime(new Faker().Date.Past()),
                Category = "cat2",
                Subcategory = "sub2",
                MerchantName = "merchant2",
            },
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, editRequests);

        // Assert
        // await act.Should()
        //     .ThrowAsync<BudgetBoardServiceException>()
        //     .WithMessage("TransactionDuplicateIdsError");
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenAmountUpdated_ShouldUpdateBalances()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[4].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        transactions[0].Date = balances[0].Date;
        transactions[0].Amount = 50.0M;
        transactions[1].Date = balances[1].Date;
        transactions[1].Amount = 30.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldLastBalance = balances.Last().Amount;

        var editRequests = new List<TransactionUpdateRequest>
        {
            new()
            {
                ID = transactions[0].ID,
                Amount = 100.0M,
                Date = transactions[0].Date,
                Category = transactions[0].Category,
                Subcategory = transactions[0].Subcategory,
                MerchantName = transactions[0].MerchantName,
            },
            new()
            {
                ID = transactions[1].ID,
                Amount = 60.0M,
                Date = transactions[1].Date,
                Category = transactions[1].Category,
                Subcategory = transactions[1].Subcategory,
                MerchantName = transactions[1].MerchantName,
            },
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, editRequests);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldLastBalance + (100.0M - 50.0M) + (60.0M - 30.0M));
    }

    [Fact]
    public async Task DeleteTransactionBatchAsync_ShouldDeleteAllTransactions()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var idsToDelete = transactions.Take(3).Select(t => t.ID).ToList();

        // Act
        await transactionService.DeleteTransactionBatchAsync(helper.demoUser.Id, idsToDelete);

        // Assert
        foreach (var id in idsToDelete)
        {
            helper
                .UserDataContext.Transactions.Single(t => t.ID == id)
                .Deleted.Should()
                .NotBeNull();
        }

        helper
            .UserDataContext.Transactions.Where(t => !idsToDelete.Contains(t.ID))
            .All(t => t.Deleted == null)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task DeleteTransactionBatchAsync_WhenAnyTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var idsToDelete = new List<Guid>
        {
            transactions.First().ID,
            Guid.NewGuid(), // does not exist
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.DeleteTransactionBatchAsync(helper.demoUser.Id, idsToDelete);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteTransactionBatchAsync_WhenDuplicateIdsInRequest_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var duplicateId = transaction.ID;

        // Act
        Func<Task> act = async () =>
            await transactionService.DeleteTransactionBatchAsync(
                helper.demoUser.Id,
                [duplicateId, duplicateId]
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionBatchDeleteDuplicateIdsError");
    }

    [Fact]
    public async Task DeleteTransactionBatchAsync_WhenDeleteTransactions_ShouldUpdateBalances()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-10));
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-5));
        balances[2].Date = DateOnly.FromDateTime(fakeDate.AddDays(-3));
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[4].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        transactions[0].Date = balances[0].Date;
        transactions[0].Amount = 50.0M;
        transactions[1].Date = balances[1].Date;
        transactions[1].Amount = 30.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldLastBalance = balances.Last().Amount;
        var idsToDelete = transactions.Select(t => t.ID).ToList();

        // Act
        await transactionService.DeleteTransactionBatchAsync(helper.demoUser.Id, idsToDelete);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldLastBalance - 50.0M - 30.0M);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenTransactionDateHasTimeComponent_ShouldUpdateSameDayBalance()
    {
        // Arrange — balance stored at midnight, transaction on the same day but at 14:30
        var fakeDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(2);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1)); // midnight the day before
        balances[1].Date = DateOnly.FromDateTime(fakeDate); // midnight on the transaction day

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();
        transaction.Date = DateOnly.FromDateTime(fakeDate); // same day as balance
        transaction.Amount = 50.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldSameDayBalance = balances[1].Amount;

        var editRequest = new TransactionUpdateRequest
        {
            ID = transaction.ID,
            Amount = 100.0M,
            Date = transaction.Date,
            Category = transaction.Category,
            Subcategory = transaction.Subcategory,
            MerchantName = transaction.MerchantName,
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editRequest]);

        // Assert — same-day balance (stored at midnight) must be updated
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldSameDayBalance + (100.0M - 50.0M));
    }

    [Fact]
    public async Task UpdateTransactionBatchAsync_WhenTransactionDateHasTimeComponent_ShouldUpdateSameDayBalance()
    {
        // Arrange — balance stored at midnight, transaction on the same day but at 14:30
        var fakeDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(2);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[1].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();
        transaction.Date = DateOnly.FromDateTime(fakeDate);
        transaction.Amount = 50.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldSameDayBalance = balances[1].Amount;

        var editRequests = new List<TransactionUpdateRequest>
        {
            new()
            {
                ID = transaction.ID,
                Amount = 100.0M,
                Date = transaction.Date,
                Category = transaction.Category,
                Subcategory = transaction.Subcategory,
                MerchantName = transaction.MerchantName,
            },
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, editRequests);

        // Assert
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldSameDayBalance + (100.0M - 50.0M));
    }

    [Fact]
    public async Task DeleteTransactionAsync_WhenTransactionDateHasTimeComponent_ShouldUpdateSameDayBalance()
    {
        // Arrange — balance stored at midnight, transaction on the same day but at 14:30
        var fakeDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(2);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[1].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();
        transaction.Date = DateOnly.FromDateTime(fakeDate);
        transaction.Amount = 50.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldSameDayBalance = balances[1].Amount;

        // Act
        await transactionService.DeleteTransactionAsync(helper.demoUser.Id, transaction.ID);

        // Assert
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldSameDayBalance - 50.0M);
    }

    [Fact]
    public async Task DeleteTransactionBatchAsync_WhenTransactionDateHasTimeComponent_ShouldUpdateSameDayBalance()
    {
        // Arrange — balance stored at midnight, transaction on the same day but at 14:30
        var fakeDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(2);

        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[1].Date = DateOnly.FromDateTime(fakeDate);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate(1).First();
        transaction.Date = DateOnly.FromDateTime(fakeDate);
        transaction.Amount = 50.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldSameDayBalance = balances[1].Amount;

        // Act
        await transactionService.DeleteTransactionBatchAsync(helper.demoUser.Id, [transaction.ID]);

        // Assert
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldSameDayBalance - 50.0M);
    }
}
