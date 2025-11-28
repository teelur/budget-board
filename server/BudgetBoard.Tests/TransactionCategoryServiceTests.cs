using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class TransactionCategoryServiceTests
{
    private readonly Faker<CategoryCreateRequest> _categoryCreateRequestFaker =
        new Faker<CategoryCreateRequest>()
            .RuleFor(c => c.Value, f => f.Random.String(20))
            .RuleFor(c => c.Parent, f => f.Random.String(20));

    private readonly Faker<CategoryUpdateRequest> _categoryUpdateRequestFaker =
        new Faker<CategoryUpdateRequest>()
            .RuleFor(c => c.Value, f => f.Random.String(20))
            .RuleFor(c => c.Parent, f => f.Random.String(20));

    [Fact]
    public async Task CreateTransactionCategoryAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                Guid.NewGuid(),
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenCalledWithValidData_ShouldCreateCategory()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var parentCategory = transactionCategoryFaker.Generate();
        parentCategory.UserID = helper.demoUser.Id;

        helper.UserDataContext.TransactionCategories.Add(parentCategory);
        helper.UserDataContext.SaveChanges();

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();
        categoryCreateRequest.Parent = parentCategory.Value;

        // Act
        await transactionCategoryService.CreateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryCreateRequest
        );

        // Assert
        helper.UserDataContext.TransactionCategories.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenCreatingDuplicate_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();
        helper.UserDataContext.TransactionCategories.Add(
            new Category
            {
                Value = categoryCreateRequest.Value,
                Parent = categoryCreateRequest.Parent,
                UserID = helper.demoUser.Id,
            }
        );
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Transaction category already exists.");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentSameAsValue_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();
        categoryCreateRequest.Parent = categoryCreateRequest.Value;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Transaction category cannot have the same name as its parent category.");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenValueEmpty_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();
        categoryCreateRequest.Value = string.Empty;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Transaction category must have a name.");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentIsDefaultCategory_ShouldNotThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();
        categoryCreateRequest.Parent = TransactionCategoriesConstants
            .DefaultTransactionCategories.Where(tc => tc.Parent.Length == 0)
            .First()
            .Value;

        // Act
        await transactionCategoryService.CreateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryCreateRequest
        );

        // Assert
        helper.UserDataContext.TransactionCategories.Should().HaveCount(1);
        helper
            .UserDataContext.TransactionCategories.Single()
            .Parent.Should()
            .Be(categoryCreateRequest.Parent);
    }

    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenCalledWithValidData_ShouldReturnCategories()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionCategoryService.ReadTransactionCategoriesAsync(
            helper.demoUser.Id
        );

        // Assert
        result.Should().BeEquivalentTo(transactionCategories.Select(t => new CategoryResponse(t)));
    }

    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenCalledWithValidDataAndCategoryGuid_ShouldReturnCategory()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await transactionCategoryService.ReadTransactionCategoriesAsync(
            helper.demoUser.Id,
            transactionCategories.First().ID
        );

        // Assert
        result.Should().BeEquivalentTo([new CategoryResponse(transactionCategories.First())]);
    }

    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenCalledWithInvalidCategoryGuid_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.ReadTransactionCategoriesAsync(
                helper.demoUser.Id,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction category you are trying to access does not exist.");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.TransactionCategories.First()
            .Value.Should()
            .Be(categoryUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithInvalidCategoryID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction category you are trying to access does not exist.");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithValidData_ShouldDeleteCategory()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        for (var i = 1; i < transactionCategories.Count; i++)
        {
            transactionCategories[i].Parent = transactionCategories.First().Value;
        }
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            transactionCategories.Last().ID
        );

        // Assert
        helper
            .UserDataContext.TransactionCategories.Should()
            .NotContainEquivalentOf(transactionCategories.Last());
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithInvalidCategoryID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.DeleteTransactionCategoryAsync(
                helper.demoUser.Id,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The transaction category you are trying to delete does not exist.");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithTransactions_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        transactions.ForEach(t => t.Category = transactionCategories.First().Value);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.DeleteTransactionCategoryAsync(
                helper.demoUser.Id,
                transactionCategories.First().ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Category is in use by transaction(s) and cannot be deleted.");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithSubcategoryWithTransactions_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);

        transactions.ForEach(t => t.Subcategory = transactionCategories.First().Value);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.DeleteTransactionCategoryAsync(
                helper.demoUser.Id,
                transactionCategories.First().ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Category is in use by transaction(s) and cannot be deleted.");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithBudgets_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(5);
        budgets.ForEach(budget => budget.UserID = helper.demoUser.Id);
        budgets.ForEach(b => b.Category = transactionCategories.First().Value);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.Budgets.AddRange(budgets);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.DeleteTransactionCategoryAsync(
                helper.demoUser.Id,
                transactionCategories.First().ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Category is in use by budget(s) and cannot be deleted.");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithSubcategories_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            Mock.Of<IStringLocalizer<ResponseStrings>>(),
            Mock.Of<IStringLocalizer<LogStrings>>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker();
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(c => c.UserID = helper.demoUser.Id);

        transactionCategories.Last().Parent = transactionCategories.First().Value;

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();
        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.DeleteTransactionCategoryAsync(
                helper.demoUser.Id,
                transactionCategories.First().ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                "Transaction category has subcategories associated with it and cannot be deleted."
            );
    }
}
