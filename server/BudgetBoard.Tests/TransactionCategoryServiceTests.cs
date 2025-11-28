using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
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
    public async Task CreateTransactionCategoryAsync_WhenCalledWithValidData_ShouldCreateCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var parentCategory = transactionCategoryFaker.Generate();

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
    public async Task CreateTransactionCategoryAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenCreatingDuplicate_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            .WithMessage("TransactionCategoryCreateDuplicateNameError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenCreatingEmptyName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            .WithMessage("TransactionCategoryCreateEmptyNameError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentSameAsValue_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            .WithMessage("TransactionCategoryCreateSameNameAsParentError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenSameNameAsParent_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var parentCategory = transactionCategoryFaker.Generate();

        helper.UserDataContext.TransactionCategories.Add(parentCategory);
        helper.UserDataContext.SaveChanges();

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
            .WithMessage("TransactionCategoryCreateSameNameAsParentError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = _categoryCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryCreateParentNotFoundError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentIsDefaultCategory_ShouldNotThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);
        transactionCategories.ForEach(tc =>
            tc.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value
        );

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;
        categoryUpdateRequest.Parent = transactionCategories.First().Parent;

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
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryUpdateNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithDuplicateName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;
        categoryUpdateRequest.Value = transactionCategories.Last().Value;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryUpdateDuplicateNameError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithEmptyName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;
        categoryUpdateRequest.Value = string.Empty;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryUpdateEmptyNameError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithSameNameAsParent_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;
        categoryUpdateRequest.Parent = categoryUpdateRequest.Value;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryUpdateSameNameAsParentError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithParentThatDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = transactionCategories.First().ID;
        categoryUpdateRequest.Parent = "NonExistentParent";

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryUpdateParentNotFoundError");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithValidData_ShouldDeleteCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);
        for (var i = 1; i < transactionCategories.Count; i++)
        {
            transactionCategories[i].Parent = transactionCategories.First().Value;
        }

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
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithTransactions_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryDeleteInUseByTransactionsError");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithSubcategoryWithTransactions_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryDeleteInUseByTransactionsError");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithBudgets_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(5);
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
            .WithMessage("TransactionCategoryDeleteInUseByBudgetsError");
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithSubcategories_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var transactionCategoryFaker = new TransactionCategoryFaker(helper.demoUser.Id);
        var transactionCategories = transactionCategoryFaker.Generate(5);

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
            .WithMessage("TransactionCategoryDeleteHasChildrenError");
    }
}
