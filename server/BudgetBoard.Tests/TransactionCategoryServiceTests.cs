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
    #region CreateTransactionCategoryAsync
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

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = "NewCategory",
            Parent = parentCategory.Value,
            CategoryType = parentCategory.CategoryType,
        };

        // Act
        await transactionCategoryService.CreateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryCreateRequest
        );

        // Assert
        helper.UserDataContext.TransactionCategories.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_InvalidUserId_ThrowsInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = "NewCategory",
            Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.First(tc => tc.Parent == string.Empty)
                .Value,
            CategoryType = "Income",
        };

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
    public async Task CreateTransactionCategoryAsync_WhenCreatingDuplicate_ShouldThrowDuplicateNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var duplicateName = "DuplicateCategory";
        helper.UserDataContext.TransactionCategories.Add(
            new Category
            {
                Value = duplicateName,
                Parent = TransactionCategoriesConstants
                    .DefaultTransactionCategories.First(tc => tc.Parent == string.Empty)
                    .Value,
                UserID = helper.demoUser.Id,
            }
        );
        helper.UserDataContext.SaveChanges();

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = duplicateName,
            Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.Shuffle()
                .First(tc => tc.Parent == string.Empty)
                .Value,
            CategoryType = "Income",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryDuplicateNameError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenCreatingEmptyName_ShouldThrowTransactionCategoryEmptyNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = string.Empty,
            Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.First(tc => tc.Parent == string.Empty)
                .Value,
            CategoryType = "Income",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryEmptyNameError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentSameAsValue_ShouldThrowTransactionCategorySameNameAsParentError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = "NewCategory",
            Parent = "NewCategory",
            CategoryType = "Income",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategorySameNameAsParentError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenParentDoesNotExist_ShouldThrowTransactionCategoryParentNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = "NewCategory",
            Parent = "NonExistentParent",
            CategoryType = "Income",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryParentNotFoundError");
    }

    [Fact]
    public async Task CreateTransactionCategoryAsync_WhenInvalidCategoryType_ShouldThrowTransactionCategoryInvalidTypeError()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionCategoryService = new TransactionCategoryService(
            Mock.Of<ILogger<ITransactionCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var categoryCreateRequest = new TransactionCategoryCreateRequest
        {
            Value = "NewCategory",
            Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.First(tc => tc.Parent == string.Empty)
                .Value,
            CategoryType = "InvalidCategoryType",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.CreateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryInvalidTypeError");
    }
    #endregion

    #region ReadTransactionCategoriesAsync
    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenCalledWithValidData_ShouldReturnCustomSpecialAndDefaultCategories()
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
        var expectedSpecialCategories =
            TransactionCategoriesConstants.SpecialTransactionCategories.Select(
                tc => new CategoryResponse(tc)
            );
        var expectedDefaultCategories =
            TransactionCategoriesConstants.DefaultTransactionCategories.Select(
                tc => new CategoryResponse(tc)
            );
        var expectedAll = expectedCustomCategories
            .Concat(expectedSpecialCategories)
            .Concat(expectedDefaultCategories);

        result.Should().BeEquivalentTo(expectedAll);
    }

    [Fact]
    public async Task ReadTransactionCategoriesAsync_WhenDefaultsDisabled_ShouldReturnSpecialAndCustomCategories()
    {
        // Arrange
        var helper = new TestHelper();

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
        var expectedSpecialCategories =
            TransactionCategoriesConstants.SpecialTransactionCategories.Select(
                tc => new CategoryResponse(tc)
            );
        var expectedAll = expectedCustomCategories.Concat(expectedSpecialCategories);

        result.Should().BeEquivalentTo(expectedAll);
        result
            .Should()
            .NotContainEquivalentOf(
                new CategoryResponse(
                    TransactionCategoriesConstants.DefaultTransactionCategories.First()
                )
            );
    }
    #endregion

    #region UpdateTransactionCategoryAsync
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
            tc.Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.Shuffle()
                .First(tc => tc.Parent == string.Empty)
                .Value
        );

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Value = "NewValue",
            Parent = transactionCategories.Last().Parent,
            CategoryType = transactionCategories.First().CategoryType,
        };

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
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithInvalidCategoryID_ShouldThrowTransactionCategoryNotFoundError()
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest { ID = Guid.NewGuid() };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithDuplicateName_ShouldThrowTransactionCategoryDuplicateNameError()
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Value = transactionCategories.Last().Value,
            Parent = transactionCategories.First().Parent,
            CategoryType = transactionCategories.First().CategoryType,
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryDuplicateNameError");
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            CategoryType = category.CategoryType,
            ID = category.ID,
            Value = category.Value,
            Parent = category.Parent,
        };

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
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithEmptyName_ShouldThrowTransactionCategoryEmptyNameError()
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Parent = transactionCategories.First().Parent,
            Value = string.Empty,
            CategoryType = transactionCategories.First().CategoryType,
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryEmptyNameError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithSameNameAsParent_ShouldThrowTransactionCategorySameNameAsParentError()
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Parent = transactionCategories.First().Parent,
            Value = transactionCategories.First().Parent,
            CategoryType = transactionCategories.First().CategoryType,
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategorySameNameAsParentError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithParentThatDoesNotExist_ShouldThrowTransactionCategoryParentNotFoundError()
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Value = transactionCategories.First().Value,
            Parent = "NonExistentParent",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryParentNotFoundError");
    }

    [Fact]
    public async Task UpdateTransactionCategoryAsync_WhenCalledWithInvalidCategoryType_ShouldThrowTransactionCategoryInvalidTypeError()
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
            tc.Parent = TransactionCategoriesConstants
                .DefaultTransactionCategories.First(dtc => string.IsNullOrEmpty(dtc.Parent))
                .Value
        );

        helper.UserDataContext.TransactionCategories.AddRange(transactionCategories);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = transactionCategories.First().ID,
            Value = transactionCategories.First().Value,
            Parent = transactionCategories.First().Parent,
            CategoryType = "InvalidCategoryType",
        };

        // Act
        Func<Task> act = async () =>
            await transactionCategoryService.UpdateTransactionCategoryAsync(
                helper.demoUser.Id,
                categoryUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TransactionCategoryInvalidTypeError");
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

        var notCategoryTransactions = transactionFaker.Generate(5);
        notCategoryTransactions.First().Subcategory = null;
        notCategoryTransactions.First().Category = null;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Transactions.AddRange(notCategoryTransactions);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            ID = category.ID,
            Value = "NewCategoryValue",
            Parent = category.Parent,
            CategoryType = category.CategoryType,
        };

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        var updatedTransactions = helper.UserDataContext.Transactions.Where(t =>
            transactions.Select(ut => ut.ID).Contains(t.ID)
        );
        updatedTransactions
            .Should()
            .AllSatisfy(t => t.Category.Should().Be(categoryUpdateRequest.Value));
        updatedTransactions
            .First(t => t.ID == transactions.First().ID)
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            Value = "NewCategoryValue",
            Parent = category.Parent,
            CategoryType = category.CategoryType,
            ID = category.ID,
        };

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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            Value = "NewCategoryValue",
            Parent = category.Parent,
            CategoryType = category.CategoryType,
            ID = category.ID,
        };

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
    public async Task UpdateTransactionCategoryAsync_WhenRenamedAndReparented_ShouldUpdateChildrenCategoryType()
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

        // Top-level category (no parent) so ResolveCategoryType uses the supplied CategoryType directly
        parentCategory.Parent = string.Empty;
        parentCategory.CategoryType = TransactionCategoryTypes.Expense;

        var children = transactionCategoryFaker.Generate(3);
        children.ForEach(c =>
        {
            c.Parent = parentCategory.Value;
            c.CategoryType = TransactionCategoryTypes.Expense;
        });

        helper.UserDataContext.TransactionCategories.Add(parentCategory);
        helper.UserDataContext.TransactionCategories.AddRange(children);
        helper.UserDataContext.SaveChanges();

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            Value = "NewParentValue",
            Parent = string.Empty,
            CategoryType = TransactionCategoryTypes.Income,
            ID = parentCategory.ID,
        };

        // Act
        await transactionCategoryService.UpdateTransactionCategoryAsync(
            helper.demoUser.Id,
            categoryUpdateRequest
        );

        // Assert
        var childIds = children.Select(c => c.ID).ToHashSet();
        helper
            .UserDataContext.TransactionCategories.Where(c => childIds.Contains(c.ID))
            .Should()
            .AllSatisfy(c => c.CategoryType.Should().Be(TransactionCategoryTypes.Income));
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

        var categoryUpdateRequest = new TransactionCategoryUpdateRequest
        {
            Value = "NewParentValue",
            CategoryType = parent.CategoryType,
            ID = parent.ID,
            Parent = parent.Parent,
        };

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
    #endregion

    #region DeleteTransactionCategoryAsync
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
    public async Task DeleteTransactionCategoryAsync_WhenCalledWithInvalidCategoryID_ShouldThrowTransactionCategoryNotFoundError()
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
            .WithMessage("TransactionCategoryNotFoundError");
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

        var subcategory = transactionCategoryFaker.Generate();
        subcategory.Parent = category.Value;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions.ForEach(t => t.Category = category.Value);
        transactions.ForEach(t => t.Subcategory = string.Empty);
        transactions.First().Subcategory = subcategory.Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.TransactionCategories.Add(subcategory);
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
        helper
            .UserDataContext.Transactions.Should()
            .AllSatisfy(t => t.Subcategory.Should().BeNull());
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

        var subcategory = transactionCategoryFaker.Generate();
        subcategory.Parent = category.Value;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(5);
        transactions.ForEach(t => t.Subcategory = subcategory.Value);
        transactions.ForEach(t => t.Category = subcategory.Parent);

        var parentTransactions = transactionFaker.Generate(5);
        parentTransactions.ForEach(t => t.Category = category.Value);

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Transactions.AddRange(parentTransactions);
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
        helper.UserDataContext.Transactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
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

        var subcategory = transactionCategoryFaker.Generate();
        subcategory.Parent = category.Value;

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(5);
        budgets.ForEach(b => b.Category = category.Value);
        budgets.First().Category = subcategory.Value;
        budgets.Last().Category = subcategory.Value;

        helper.UserDataContext.TransactionCategories.Add(category);
        helper.UserDataContext.TransactionCategories.Add(subcategory);
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
    #endregion
}
