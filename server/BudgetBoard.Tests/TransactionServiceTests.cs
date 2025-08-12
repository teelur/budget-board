﻿using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class TransactionServiceTests
{
    private readonly Faker<TransactionCreateRequest> _transactionCreateRequestFaker =
        new Faker<TransactionCreateRequest>()
            .RuleFor(t => t.SyncID, f => f.Random.String(20))
            .RuleFor(t => t.Amount, f => f.Finance.Amount())
            .RuleFor(t => t.Date, f => f.Date.Past())
            .RuleFor(t => t.Category, f => f.Random.String(10))
            .RuleFor(t => t.Subcategory, f => f.Random.String(10))
            .RuleFor(t => t.MerchantName, f => f.Random.String(10))
            .RuleFor(t => t.Source, f => f.Random.String(10));

    [Fact]
    public async Task CreateTransactionAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var transaction = _transactionCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await transactionService.CreateTransactionAsync(Guid.NewGuid(), transaction);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldCreateTransaction()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = _transactionCreateRequestFaker.Generate();
        transaction.AccountID = account.ID;
        transaction.Date = transaction.Date.ToUniversalTime();

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);

        // Assert
        helper.UserDataContext.Transactions.Should().ContainSingle();
        helper.UserDataContext.Transactions.Single().Should().BeEquivalentTo(transaction);
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenAccountDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var transaction = _transactionCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The account you are trying to add a transaction to does not exist.");
    }

    [Fact]
    public async Task CreateTransactionAsync_WhenNewTransaction_ShouldUpdateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.Source = AccountSource.Manual;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = _transactionCreateRequestFaker.Generate();
        transaction.AccountID = account.ID;

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);
        // Assert
        helper.UserDataContext.Balances.Should().ContainSingle();
        helper
            .UserDataContext.Balances.Single()
            .DateTime.Should()
            .Be(transaction.Date.ToUniversalTime());
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
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker() { AccountIds = [account.ID] };
        var balances = balanceFaker.Generate(5);

        balances[0].DateTime = fakeDate.AddDays(-10);
        balances[1].DateTime = fakeDate.AddDays(-5);
        balances[2].DateTime = fakeDate.AddDays(-3);
        balances[3].DateTime = fakeDate.AddDays(-1);
        balances[4].DateTime = fakeDate;

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transaction = _transactionCreateRequestFaker.Generate();
        transaction.AccountID = account.ID;
        transaction.Date = fakeDate.AddDays(-2);

        var oldBalance = balances[4].Amount;

        // Act
        await transactionService.CreateTransactionAsync(helper.demoUser.Id, transaction);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(6);
        helper.UserDataContext.Balances.ToList().ElementAt(4).Should().NotBeNull();
        helper
            .UserDataContext.Balances.ToList()
            .ElementAt(4)
            .DateTime.Should()
            .Be(balances[4].DateTime);
        helper
            .UserDataContext.Balances.ToList()
            .ElementAt(4)
            .Amount.Should()
            .Be(oldBalance + transaction.Amount);
    }

    [Fact]
    public async Task ReadTransactionsAsync_ShouldReturnTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
    public async Task ReadTransactionsAsync_WhenTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.ReadTransactionsAsync(
                helper.demoUser.Id,
                null,
                null,
                false,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction you are trying to access does not exist.");
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
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
        result.Should().HaveCount(transactions.Where(t => t.Date.Year == fakeDate.Year).Count());
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
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
        result.Should().HaveCount(transactions.Where(t => t.Date.Month == fakeDate.Month).Count());
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
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
    public async Task UpdateTransactionAsync_ShouldUpdateTransaction()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = transactions.First().ID,
            Amount = 100.0M,
            Date = new Faker().Date.Past().ToUniversalTime(),
            Category = "newCategory",
            Subcategory = "newSubcategory",
            MerchantName = "newMerchantName",
        };

        // Act
        await transactionService.UpdateTransactionAsync(helper.demoUser.Id, editedTransaction);

        // Assert
        helper
            .demoUser.Accounts.SelectMany(a => a.Transactions)
            .First(t => t.ID == editedTransaction.ID)
            .Should()
            .BeEquivalentTo(editedTransaction);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenTransactionDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = Guid.NewGuid(),
            Amount = 100.0M,
            Date = new Faker().Date.Past(),
            Category = "newCategory",
            Subcategory = "newSubcategory",
            MerchantName = "newMerchantName",
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.UpdateTransactionAsync(helper.demoUser.Id, editedTransaction);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction you are trying to edit does not exist.");
    }

    [Fact]
    public async Task UpdateTransactionAsync_WhenAmountUpdated_ShouldUpdateBalance()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker() { AccountIds = [account.ID] };
        var balances = balanceFaker.Generate(5);

        balances[0].DateTime = fakeDate.AddDays(-10);
        balances[1].DateTime = fakeDate.AddDays(-5);
        balances[2].DateTime = fakeDate.AddDays(-3);
        balances[3].DateTime = fakeDate.AddDays(-1);
        balances[4].DateTime = fakeDate;

        account.Balances = balances;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
        var transactions = transactionFaker.Generate(2);

        transactions.First().Date = balances.First().DateTime;
        transactions.First().Amount = 50.0M;

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedTransaction = new TransactionUpdateRequest
        {
            ID = transactions.First().ID,
            Amount = 100.0M,
            Date = transactions.First().Date,
            Category = transactions.First().Category,
            Subcategory = transactions.First().Subcategory,
            MerchantName = transactions.First().MerchantName,
        };

        var oldBalance = balances.Last().Amount;

        // Act
        await transactionService.UpdateTransactionAsync(helper.demoUser.Id, editedTransaction);

        // Assert
        helper.UserDataContext.Balances.Should().HaveCount(5);
        helper.UserDataContext.Balances.ToList().Last().Should().NotBeNull();
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .DateTime.Should()
            .Be(balances.Last().DateTime);
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .Amount.Should()
            .Be(oldBalance - 50.0M + 100.0M);
    }

    [Fact]
    public async Task DeleteTransactionAsync_ShouldDeleteTransaction()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.DeleteTransactionAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction you are trying to delete does not exist.");
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
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.Source = AccountSource.Manual;

        var balanceFaker = new BalanceFaker() { AccountIds = [account.ID] };
        var balances = balanceFaker.Generate(5);

        balances[0].DateTime = fakeDate.AddDays(-10);
        balances[1].DateTime = fakeDate.AddDays(-5);
        balances[2].DateTime = fakeDate.AddDays(-3);
        balances[3].DateTime = fakeDate.AddDays(-1);
        balances[4].DateTime = fakeDate;

        account.Balances = balances;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
        var transactions = transactionFaker.Generate(2);
        transactions[0].Date = balances[0].DateTime;

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
        helper
            .UserDataContext.Balances.ToList()
            .Last()
            .DateTime.Should()
            .Be(balances.Last().DateTime);
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
            nowProviderMock.Object
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await transactionService.RestoreTransactionAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction you are trying to restore does not exist.");
    }

    [Fact]
    public async Task RestoreTransactionAsync_WhenTransactionIsNotDeleted_ShouldNotRestoreTransaction()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
        var transactions = transactionFaker.Generate(5);

        account.Transactions = transactions;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var transactionToRestore = transactions.First();

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
    public async Task SplitTransactionAsync_WhenSplitTransaction_ShouldSplitTransaction()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();

        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
            Mock.Of<INowProvider>()
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
            .WithMessage("The transaction you are trying to split does not exist.");
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
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();

        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
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
            .WithMessage("The split amount must be less than the transaction amount.");
    }

    [Fact]
    public async Task ImportTransactionsAsync_WhenValidData_ShouldImportTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var transactionFaker = new TransactionFaker() { AccountIds = [account.ID] };
        var transactions = transactionFaker.Generate(5);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var importRequest = new TransactionImportRequest
        {
            Transactions = transactions.Select(t => new TransactionImport
            {
                Date = t.Date,
                Description = t.MerchantName ?? string.Empty,
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
}
