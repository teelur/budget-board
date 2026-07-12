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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
    public async Task CreateTransactionAsync_WhenNewTransactionAndManualAccountWithNoBalance_ShouldCreateBalanceForThatDate()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        balances[2].Amount = 200.0M;
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[3].Amount = 250.0M;
        balances[4].Date = DateOnly.FromDateTime(fakeDate);
        balances[4].Amount = 300.0M;

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = new TransactionCreateRequest
        {
            Amount = 100.0M,
            Date = DateOnly.FromDateTime(fakeDate.AddDays(-6)),
            Category = "TestCategory",
            Subcategory = "TestSubcategory",
            MerchantName = "TestMerchant",
            AccountID = account.ID,
        };

        var oldBalances = balances.Select(b => b.Amount).ToList();

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(6);
        var unchangedBalance = balances[0];
        unchangedBalance.Amount.Should().Be(oldBalances[0]);
        var newBalance = helper.UserDataContext.Balances.Single(b => b.Date == transaction.Date);
        newBalance.Amount.Should().Be(unchangedBalance.Amount + transaction.Amount);
        var updatedBalances = balances.Where(b => b.Date > transaction.Date).ToList();
        for (int i = 0; i < updatedBalances.Count; i++)
        {
            var balance = updatedBalances[i];
            if (balance.Date <= transaction.Date)
                continue;

            var updatedBalance = updatedBalances.Single(b => b.Date == balance.Date);
            var oldBalanceAmount = oldBalances[i + 1];
            updatedBalance.Amount.Should().Be(oldBalanceAmount + transaction.Amount);
        }
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions[0].Source = TransactionSource.Manual.ToString();
        transactions[1].Source = TransactionSource.Manual.ToString();

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            false,
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            false,
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            false,
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        var secondAccount = accountFaker.Generate();
        var transactionsForSecondAccount = transactionFaker.Generate(5);
        transactionsForSecondAccount.ForEach(t =>
            t.Category = TransactionCategoriesConstants.HideFromBudgetsCategory
        );
        transactionsForSecondAccount.ForEach(t => t.Subcategory = string.Empty);

        secondAccount.Transactions = transactionsForSecondAccount;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Accounts.Add(secondAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            true,
            false
        );

        // Assert
        result.Should().HaveCount(10);
        result
            .Should()
            .BeEquivalentTo(
                transactions
                    .Select(t => new TransactionResponse(t))
                    .Concat(transactionsForSecondAccount.Select(t => new TransactionResponse(t)))
            );
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;
        account.HideTransactions = true;

        var secondAccount = accountFaker.Generate();
        var transactionsForSecondAccount = transactionFaker.Generate(5);
        transactionsForSecondAccount.ForEach(t =>
            t.Category = TransactionCategoriesConstants.HideFromBudgetsCategory
        );
        transactionsForSecondAccount.ForEach(t => t.Subcategory = string.Empty);

        secondAccount.Transactions = transactionsForSecondAccount;
        secondAccount.HideTransactions = false;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Accounts.Add(secondAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionService.ReadTransactionsAsync(
            helper.demoUser.Id,
            null,
            null,
            false,
            false
        );

        // Assert
        result.Should().BeEmpty();
    }
    #endregion

    #region UpdateTransactionsAsync
    [Fact]
    public async Task UpdateTransactionsAsync_WhenValidData_ShouldUpdateTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
    public async Task UpdateTransactionsAsync_WhenMultipleTransactions_ShouldUpdateAllTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        foreach (var request in editRequests)
        {
            var updatedTransaction = helper.UserDataContext.Transactions.Single(t =>
                t.ID == request.ID
            );
            updatedTransaction.Amount.Should().Be(request.Amount);
            updatedTransaction.Date.Should().Be(request.Date);
            updatedTransaction.Category.Should().Be(request.Category.Value);
            updatedTransaction.Subcategory.Should().Be(request.Subcategory.Value);
            updatedTransaction.MerchantName.Should().Be(request.MerchantName.Value);
        }
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
    public async Task UpdateTransactionsAsync_WhenAmountUpdated_ShouldUpdateBalancesOnAndAfterThatDate()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
    public async Task UpdateTransactionsAsync_WhenDateUpdated_ShouldMoveBalanceImpactToNewDate()
    {
        // Arrange
        var fakeDate = new DateOnly(2025, 1, 4);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.Today).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
                Amount = 200m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 4),
                Amount = 250m,
            },
        ];

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.Date = new DateOnly(2025, 1, 4);
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
        balances.Should().HaveCount(4);
        balances[0].Amount.Should().Be(100m);
        balances[1].Amount.Should().Be(200m);
        balances[2].Amount.Should().Be(250m);
        balances[3].Amount.Should().Be(250m);
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenDateAndAmountUpdated_ShouldMoveUpdatedBalanceImpact()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
                Amount = 200m,
            },
            new Balance
            {
                AccountID = account.ID,
                Date = new DateOnly(2025, 1, 4),
                Amount = 250m,
            },
        ];

        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Date = new DateOnly(2025, 1, 4);
        transaction.Amount = 50m;
        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editRequest = new TransactionUpdateRequest
        {
            ID = transaction.ID,
            Amount = 75m,
            Date = new DateOnly(2025, 1, 2),
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editRequest]);

        // Assert
        var balances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        balances.Select(b => b.Amount).Should().Equal(100m, 225m, 275m, 275m);
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenMovedToDateWithoutBalance_ShouldCreateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        balances.Should().HaveCount(3);
        balances
            .Select(b => b.Date)
            .Should()
            .Equal(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 2), new DateOnly(2025, 1, 3));
        balances.Select(b => b.Amount).Should().Equal(100m, 150m, 150m);
    }

    [Fact]
    public async Task UpdateTransactionsAsync_WhenAccountIsNotManual_ShouldNotUpdateBalances()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.SimpleFIN;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(5);

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(2);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.AddRange(balances);
        helper.UserDataContext.SaveChanges();

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = transactions.First().ID,
            Amount = 100.0M,
        };

        // Act
        await transactionService.UpdateTransactionsAsync(helper.demoUser.Id, [editedTransaction]);

        // Assert
        var updatedTransaction = helper.UserDataContext.Transactions.Single(t =>
            t.ID == transactions[0].ID
        );
        updatedTransaction.Amount.Should().Be(100m);
    }
    #endregion

    #region DeleteTransactionsAsync
    [Fact]
    public async Task DeleteTransactionsAsync_WhenValidData_ShouldDeleteTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        await transactionService.DeleteTransactionsAsync(
            helper.demoUser.Id,
            [transactionToDelete.ID]
        );

        // Assert
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transactionToDelete.ID)
            .Deleted.Should()
            .NotBeNull();
    }

    [Fact]
    public async Task DeleteTransactionsAsync_WhenMultipleTransactions_ShouldDeleteAllTransactions()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        await transactionService.DeleteTransactionsAsync(helper.demoUser.Id, idsToDelete);

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
    public async Task DeleteTransactionsAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.DeleteTransactionsAsync(helper.demoUser.Id, [Guid.NewGuid()]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionNotFoundError");
    }

    [Fact]
    public async Task DeleteTransactionsAsync_WhenDeleteTransaction_ShouldUpdateBalance()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        balances[2].Amount = 200.0M;
        balances[3].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[3].Amount = 250.0M;
        balances[4].Date = DateOnly.FromDateTime(fakeDate);
        balances[4].Amount = 300.0M;

        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.Date = balances[0].Date;
        transaction.Amount = 50.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var oldBalances = balances.Select(b => b.Amount).ToList();

        // Act
        await transactionService.DeleteTransactionsAsync(helper.demoUser.Id, [transaction.ID]);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        var updatedBalances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        updatedBalances
            .Select(b => b.Amount)
            .Should()
            .Equal(oldBalances.Select(b => b - transaction.Amount));
    }

    [Fact]
    public async Task DeleteTransactionsAsync_WhenAccountIsNotManual_ShouldNotUpdateBalances()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.SimpleFIN;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(3);
        balances[0].Amount = 100m;
        balances[1].Amount = 150m;
        balances[2].Amount = 200m;
        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.Category = "OriginalCategory";
        transaction.Subcategory = "OriginalSubcategory";
        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionService.DeleteTransactionsAsync(helper.demoUser.Id, [transaction.ID]);

        // Assert
        helper
            .UserDataContext.Balances.Should()
            .OnlyContain(b => b.Amount == 100m || b.Amount == 150m || b.Amount == 200m);
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transaction.ID)
            .Deleted.Should()
            .NotBeNull();
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transaction.ID)
            .Category.Should()
            .BeNull();
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transaction.ID)
            .Subcategory.Should()
            .BeNull();
    }
    #endregion

    #region RestoreTransactionsAsync
    [Fact]
    public async Task RestoreTransactionsAsync_WhenTransactionIsDeleted_ShouldRestoreTransaction()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
        await transactionService.RestoreTransactionsAsync(
            helper.demoUser.Id,
            [transactionToRestore.ID]
        );

        // Assert
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transactionToRestore.ID)
            .Deleted.Should()
            .BeNull();
    }

    [Fact]
    public async Task RestoreTransactionsAsync_WhenTransactionDoesNotExist_ShouldThrowTransactionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.RestoreTransactionsAsync(helper.demoUser.Id, [Guid.NewGuid()]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionNotFoundError");
    }

    [Fact]
    public async Task RestoreTransactionsAsync_WhenManualAccountTransactionIsRestored_ShouldRestoreBalance()
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(3);
        balances[0].Date = DateOnly.FromDateTime(fakeDate.AddDays(-2));
        balances[0].Amount = 100m;
        balances[1].Date = DateOnly.FromDateTime(fakeDate.AddDays(-1));
        balances[1].Amount = 150m;
        balances[2].Date = DateOnly.FromDateTime(fakeDate);
        balances[2].Amount = 200m;
        account.Balances = balances;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.Date = balances[1].Date;
        transaction.Amount = 25m;
        transaction.Deleted = fakeDate;
        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionService.RestoreTransactionsAsync(helper.demoUser.Id, [transaction.ID]);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(3);
        var restoredBalances = helper.UserDataContext.Balances.OrderBy(b => b.Date).ToList();
        restoredBalances[0].Amount.Should().Be(100m);
        restoredBalances[1].Amount.Should().Be(175m);
        restoredBalances[2].Amount.Should().Be(225m);
        helper
            .UserDataContext.Transactions.Single(t => t.ID == transaction.ID)
            .Deleted.Should()
            .BeNull();
    }
    #endregion

    #region SplitTransactionAsync
    [Fact]
    public async Task SplitTransactionAsync_WhenSplitTransaction_ShouldSplitTransaction()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();

        transaction.Amount = 100.0M;

        account.Transactions = [transaction];

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToSplitAmount = transaction.Amount;
        var splitTransactionRequest = new TransactionSplitRequest
        {
            ID = transaction.ID,
            Amount = 20.0M,
            Category = "test",
            Subcategory = "test2",
        };

        // Act
        await transactionService.SplitTransactionAsync(helper.demoUser.Id, splitTransactionRequest);

        // Assert
        helper.UserDataContext.Transactions.Should().HaveCount(2);
        var existingTransaction = helper.UserDataContext.Transactions.Single(t =>
            t.ID == transaction.ID
        );
        existingTransaction
            .Amount.Should()
            .Be(transactionToSplitAmount - splitTransactionRequest.Amount);

        var newTransaction = helper.UserDataContext.Transactions.Single(t =>
            t.ID != transaction.ID
        );
        newTransaction.Amount.Should().Be(splitTransactionRequest.Amount);
        newTransaction.Category.Should().Be(splitTransactionRequest.Category);
        newTransaction.Subcategory.Should().Be(splitTransactionRequest.Subcategory);
        newTransaction.Date.Should().Be(transaction.Date);
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            .WithMessage("TransactionNotFoundError");
    }

    [Theory]
    [InlineData(100.0, 200.0)]
    [InlineData(-100.0, -150.0)]
    public async Task SplitTransactionAsync_WhenSplitAmountTooLarge_ShouldThrowTransactionSplitInvalidAmountError(
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
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
    #endregion

    #region ImportTransactionsAsync
    [Fact]
    public async Task ImportTransactionsAsync_WhenValidData_ShouldImportTransactions()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        var importRequest = new TransactionImportRequest
        {
            Transactions = transactions.Select(t => new TransactionImport
            {
                Date = t.Date,
                MerchantName = t.MerchantName ?? string.Empty,
                Category = "Auto & Transport",
                Amount = t.Amount,
                Account = "bongus",
            }),
            AccountNameToIDMap = [new() { AccountName = "bongus", AccountID = account.ID }],
        };

        // Act
        await transactionService.ImportTransactionsAsync(helper.demoUser.Id, importRequest);

        // Assert
        helper.UserDataContext.Transactions.Should().HaveCount(5);
        foreach (var transaction in helper.UserDataContext.Transactions)
        {
            var importedTransaction = helper.UserDataContext.Transactions.Single(t =>
                t.ID == transaction.ID
            );
            importedTransaction.Date.Should().Be(transaction.Date);
            importedTransaction.MerchantName.Should().Be(transaction.MerchantName);
            importedTransaction.Category.Should().Be(transaction.Category);
            importedTransaction.Amount.Should().Be(transaction.Amount);
            importedTransaction.AccountID.Should().Be(account.ID);
        }
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenAccountNameUsesDifferentCasingAndDateAndAmountAreNull_ShouldUseDefaultsAndImportTransaction()
    {
        // Arrange
        var fakeDate = new DateOnly(2025, 1, 14);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.Today).Returns(fakeDate);

        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.Manual;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var importRequest = new TransactionImportRequest
        {
            Transactions =
            [
                new TransactionImport
                {
                    Date = null,
                    MerchantName = "ImportedWithoutDate",
                    Category = "unknown",
                    Amount = null,
                    Account = "bongus",
                },
            ],
            AccountNameToIDMap = [new() { AccountName = "BONGUS", AccountID = account.ID }],
        };

        // Act
        await transactionService.ImportTransactionsAsync(helper.demoUser.Id, importRequest);

        // Assert
        var importedTransaction = helper.UserDataContext.Transactions.Single();
        importedTransaction.Date.Should().Be(fakeDate);
        importedTransaction.Amount.Should().Be(0m);
        importedTransaction.MerchantName.Should().Be("ImportedWithoutDate");
        importedTransaction.AccountID.Should().Be(account.ID);
        importedTransaction.Category.Should().Be(string.Empty);
        helper.UserDataContext.Balances.Should().ContainSingle();
        helper.UserDataContext.Balances.Single().Date.Should().Be(fakeDate);
        helper.UserDataContext.Balances.Single().Amount.Should().Be(0m);
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenCategoryNotFound_ShouldAddWithEmptyCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        var importRequest = new TransactionImportRequest
        {
            Transactions = transactions.Select(t => new TransactionImport
            {
                Date = t.Date,
                MerchantName = t.MerchantName ?? string.Empty,
                Category = "bongus",
                Amount = t.Amount,
                Account = "bongus",
            }),
            AccountNameToIDMap = [new() { AccountName = "bongus", AccountID = account.ID }],
        };

        // Act
        await transactionService.ImportTransactionsAsync(helper.demoUser.Id, importRequest);

        // Assert
        helper.UserDataContext.Transactions.Should().HaveCount(5);
        foreach (var transaction in helper.UserDataContext.Transactions)
        {
            var importedTransaction = helper.UserDataContext.Transactions.Single(t =>
                t.ID == transaction.ID
            );
            importedTransaction.Category.Should().Be(string.Empty);
        }
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenAccountNotFound_ShouldThrowTransactionAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
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
            .WithMessage("TransactionAccountNotFoundError");
    }
    #endregion
}
