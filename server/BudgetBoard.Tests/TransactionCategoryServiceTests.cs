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
    private readonly Faker<TransactionCategoryCreateRequest> _categoryCreateRequestFaker =
        new Faker<TransactionCategoryCreateRequest>()
            .RuleFor(c => c.Value, f => f.Random.String(20))
            .RuleFor(c => c.Parent, f => f.Random.String(20));

    private readonly Faker<TransactionCategoryUpdateRequest> _categoryUpdateRequestFaker =
        new Faker<TransactionCategoryUpdateRequest>()
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
    public async Task ReadTransactionCategoriesAsync_WhenCalledWithValidData_ShouldReturnCustomAndDefaultCategories()
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
        var expectedCustomCategories = transactionCategories.Select(t => new CategoryResponse(t));
        var expectedDefaultCategories =
            TransactionCategoriesConstants.DefaultTransactionCategories.Select(
                tc => new CategoryResponse(tc)
            );
        var expectedAll = expectedCustomCategories.Concat(expectedDefaultCategories);

        result.Should().BeEquivalentTo(expectedAll);
    }

    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenDefaultsDisabled_ShouldReturnOnlyCustomCategories()
    {
        // Arrange
        var helper = new TestHelper();

        // Disable built-in categories
        var userSettings = new UserSettings
        {
            UserID = helper.demoUser.Id,
            DisableBuiltInTransactionCategories = true,
        };
        helper.UserDataContext.UserSettings.Add(userSettings);
        helper.demoUser.UserSettings = userSettings;
        helper.UserDataContext.SaveChanges();

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
        var expectedCustomCategories = transactionCategories.Select(t => new CategoryResponse(t));
        result.Should().BeEquivalentTo(expectedCustomCategories);
        result
            .Should()
            .NotContainEquivalentOf(
                new CategoryResponse(
                    TransactionCategoriesConstants.DefaultTransactionCategories.First()
                )
            );
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
    public async Task UpdateTransactionCategoryAsync_WhenRenamed_ShouldUpdateTransactionCategories()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions.ForEach(t => t.Category = category.Value);
        transactions.First().Subcategory = category.Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = category.ID;
        categoryUpdateRequest.Parent = category.Parent;

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.Transactions.Should()
            .AllSatisfy(t => t.Category.Should().Be(categoryUpdateRequest.Value));
        helper
            .UserDataContext.Transactions.First()
            .Subcategory.Should()
            .Be(categoryUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenRenamed_ShouldUpdateBudgets()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value;

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(5);
        budgets.ForEach(b => b.Category = category.Value);

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Budgets.AddRange(budgets);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = category.ID;
        categoryUpdateRequest.Parent = category.Parent;

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.Budgets.Should()
            .AllSatisfy(b => b.Category.Should().Be(categoryUpdateRequest.Value));
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenRenamed_ShouldUpdateRuleActions()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value;

        var rule = new AutomaticRuleFaker(helper.demoUser.Id).Generate();
        var categoryAction = rule.Actions.First();
        categoryAction.Field = AutomaticRuleConstants.TransactionFields.Category;
        categoryAction.Value = category.Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.AutomaticRules.Add(rule);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = category.ID;
        categoryUpdateRequest.Parent = category.Parent;

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.RuleActions.First(a => a.ID == categoryAction.ID)
            .Value.Should()
            .Be(categoryUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenRenamed_ShouldUpdateChildrenParent()
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
        var parent = transactionCategoryFaker.Generate();
        parent.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value;

        var children = transactionCategoryFaker.Generate(3);
        children.ForEach(c => c.Parent = parent.Value);

        helper.UserDataContext.TransactionCategories.Add(parent);
        helper.UserDataContext.TransactionCategories.AddRange(children);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = parent.ID;
        categoryUpdateRequest.Parent = parent.Parent;

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.TransactionCategories.Where(c => c.ID != parent.ID)
            .Should()
            .AllSatisfy(c => c.Parent.Should().Be(categoryUpdateRequest.Value));
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenUpdatedWithSameName_ShouldNotThrowDuplicate()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = _categoryUpdateRequestFaker.Generate();
        categoryUpdateRequest.ID = category.ID;
        categoryUpdateRequest.Value = category.Value;
        categoryUpdateRequest.Parent = category.Parent;

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should().NotThrowAsync();
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
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithTransactions_ShouldNullOutTransactionCategories()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = string.Empty;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions.ForEach(t => t.Category = category.Value);

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            category.ID
        );

        // Assert
        helper.UserDataContext.Transactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithSubcategoryWithTransactions_ShouldNullOutTransactionSubcategories()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = string.Empty;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions.ForEach(t => t.Subcategory = category.Value);

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            category.ID
        );

        // Assert
        helper
            .UserDataContext.Transactions.Should()
            .AllSatisfy(t => t.Subcategory.Should().BeNull());
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryWithBudgets_ShouldRemoveBudgets()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = string.Empty;

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(5);
        budgets.ForEach(b => b.Category = category.Value);

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Budgets.AddRange(budgets);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            category.ID
        );

        // Assert
        helper.UserDataContext.Budgets.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithParentCategory_ShouldDeleteChildren()
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
        var parent = transactionCategoryFaker.Generate();
        parent.Parent = string.Empty;

        var children = transactionCategoryFaker.Generate(3);
        children.ForEach(c => c.Parent = parent.Value);

        helper.UserDataContext.TransactionCategories.Add(parent);
        helper.UserDataContext.TransactionCategories.AddRange(children);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            parent.ID
        );

        // Assert
        helper.UserDataContext.TransactionCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithCategoryUsedInRuleAction_ShouldClearRuleActionValue()
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
        var category = transactionCategoryFaker.Generate();
        category.Parent = string.Empty;

        var rule = new AutomaticRuleFaker(helper.demoUser.Id).Generate();
        var categoryAction = rule.Actions.First();
        categoryAction.Field = AutomaticRuleConstants.TransactionFields.Category;
        categoryAction.Value = category.Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.AutomaticRules.Add(rule);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            category.ID
        );

        // Assert
        helper
            .UserDataContext.RuleActions.First(a => a.ID == categoryAction.ID)
            .Value.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithParentCategoryWithChildUsedInRuleAction_ShouldClearChildRuleActionValue()
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
        var parent = transactionCategoryFaker.Generate();
        parent.Parent = string.Empty;

        var child = transactionCategoryFaker.Generate();
        child.Parent = parent.Value;

        var rule = new AutomaticRuleFaker(helper.demoUser.Id).Generate();
        var categoryAction = rule.Actions.First();
        categoryAction.Field = AutomaticRuleConstants.TransactionFields.Category;
        categoryAction.Value = child.Value;

        helper.UserDataContext.TransactionCategories.Add(parent);
        helper.UserDataContext.TransactionCategories.Add(child);
        helper.UserDataContext.AutomaticRules.Add(rule);
        helper.UserDataContext.SaveChanges();

        // Act
        await transactionCategoryService.DeleteTransactionCategoryAsync(
            helper.demoUser.Id,
            parent.ID
        );

        // Assert
        helper
            .UserDataContext.RuleActions.First(a => a.ID == categoryAction.ID)
            .Value.Should()
            .BeEmpty();
    }
}
