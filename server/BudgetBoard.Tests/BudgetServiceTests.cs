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
public class BudgetServiceTests
{
    #region CreateBudgetsAsync
    [Fact]
    public async Task CreateBudgetsAsync_WhenValidData_ShouldCreateBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget1 = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 1000,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };
        var budget2 = new BudgetCreateRequest
        {
            Category = "Bonus",
            Limit = 500,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget1, budget2]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        var firstBudget = helper.UserDataContext.Budgets.FirstOrDefault(b =>
            b.Category == budget1.Category
        );
        firstBudget.Should().NotBeNull();
        firstBudget.Limit.Should().Be(budget1.Limit);
        firstBudget.Month.Should().Be(budget1.Month);
        var secondBudget = helper.UserDataContext.Budgets.FirstOrDefault(b =>
            b.Category == budget2.Category
        );
        secondBudget.Should().NotBeNull();
        secondBudget.Limit.Should().Be(budget2.Limit);
        secondBudget.Month.Should().Be(budget2.Month);
    }

    [Fact]
    public async Task CreateBudgetsAsync_AutoManageParents_WhenValidData_ShouldCreateBudgetsWithParents()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var today = DateOnly.FromDateTime(DateTime.Today);
        var budgets = new List<BudgetCreateRequest>();
        var newBudget1 = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 1000,
            Month = today,
        };
        budgets.Add(newBudget1);
        var newBudget2 = new BudgetCreateRequest
        {
            Category = "Bonus",
            Limit = 3000,
            Month = today,
        };
        budgets.Add(newBudget2);
        var newBudget3 = new BudgetCreateRequest
        {
            Category = "Service & Parts",
            Limit = 2000,
            Month = today,
        };
        budgets.Add(newBudget3);

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, budgets, true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(5);
        var parentBudget1 = helper.UserDataContext.Budgets.Single(b => b.Category == "Income");
        parentBudget1.Limit.Should().Be(newBudget1.Limit + newBudget2.Limit);
        parentBudget1.Month.Should().Be(today);
        var childBudget1 = helper.UserDataContext.Budgets.Single(b => b.Category == "Paycheck");
        childBudget1.Limit.Should().Be(newBudget1.Limit);
        childBudget1.Month.Should().Be(today);
        var childBudget2 = helper.UserDataContext.Budgets.Single(b => b.Category == "Bonus");
        childBudget2.Limit.Should().Be(newBudget2.Limit);
        childBudget2.Month.Should().Be(today);
        var parentBudget2 = helper.UserDataContext.Budgets.Single(b =>
            b.Category == "Auto & Transport"
        );
        parentBudget2.Limit.Should().Be(newBudget3.Limit);
        parentBudget2.Month.Should().Be(today);
        var childBudget3 = helper.UserDataContext.Budgets.Single(b =>
            b.Category == "Service & Parts"
        );
        childBudget3.Limit.Should().Be(newBudget3.Limit);
        childBudget3.Month.Should().Be(today);
    }

    [Fact]
    public async Task CreateBudgetsAsync_AutoManageParents_WhenCategoryIsParent_ShouldNotAddAnother()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget = new BudgetCreateRequest
        {
            Category = "Income",
            Limit = 1000,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
        var parentBudget = helper.UserDataContext.Budgets.First(b => b.Category == budget.Category);
        parentBudget.Limit.Should().Be(budget.Limit);
        parentBudget.Month.Should().Be(budget.Month);
    }

    [Fact]
    public async Task CreateBudgetsAsync_AutoManageParents_WhenCategoryHasNoParent_ShouldNotDoubleBudgetLimit()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget = new BudgetCreateRequest
        {
            Category = "Auto & Transport",
            Limit = 100,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
        var createdBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == "Auto & Transport"
        );
        createdBudget.Category.Should().NotBeNull();
        createdBudget.Month.Should().Be(budget.Month);
        createdBudget.Limit.Should().Be(100);
    }

    [Fact]
    public async Task CreateBudgetsAsync_AutoManageParents_WhenUnknownParentCategory_ShouldThrowFromParentCreation()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var testCategory = new Category
        {
            Value = "Test",
            Parent = "Parent Category That Does Not Exist",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.TransactionCategories.Add(testCategory);
        await helper.UserDataContext.SaveChangesAsync();

        var budget = new BudgetCreateRequest
        {
            Category = "Test",
            Limit = 123,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                "BudgetCreateCompletedWithErrorsError [BudgetCreateCategoryNotFoundError [Parent Category That Does Not Exist]]"
            );

        helper.UserDataContext.Budgets.Should().HaveCount(1);
        var createdBudget = helper.UserDataContext.Budgets.Single();
        createdBudget.Category.Should().Be(budget.Category);
        createdBudget.Limit.Should().Be(budget.Limit);
    }

    [Fact]
    public async Task CreateBudgetsAsync_InvalidUserGuid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 1000,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(Guid.NewGuid(), [budget]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenDuplicateBudget_ShouldThrowBudgetCreateDuplicateError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var today = DateOnly.FromDateTime(DateTime.Today);
        var category = "Food & Dining";

        var budget1 = new BudgetCreateRequest
        {
            Category = category,
            Limit = 1000,
            Month = today,
        };
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget1], false);

        var budget2 = new BudgetCreateRequest
        {
            Category = category,
            Limit = 500,
            Month = today,
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget2], false);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                $"BudgetCreateCompletedWithErrorsError [BudgetCreateDuplicateError [{category}, {today:yyyy-MM}]]"
            );
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenBudgetForCategoryAlreadyExists_ShouldThrowBudgetCreateDuplicateError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var today = DateOnly.FromDateTime(DateTime.Today);
        var category = "Food & Dining";

        var budget1 = new BudgetCreateRequest
        {
            Category = category,
            Limit = 1000,
            Month = today,
        };
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget1], false);

        var budget2 = new BudgetCreateRequest
        {
            Category = category,
            Limit = 500,
            Month = today,
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget2], false);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                $"BudgetCreateCompletedWithErrorsError [BudgetCreateDuplicateError [{category}, {today:yyyy-MM}]]"
            );
    }

    [Fact]
    public async Task CreateBudgetsAsync_AutoManageParents_WhenCreateChildAndChildAlreadyExists_ShouldCreateParentWithSumOfLimits()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var child1Budget = budgetFaker.Generate();
        child1Budget.Category = "Bonus";
        child1Budget.Limit = 1000;
        child1Budget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(child1Budget);

        var child2Budget = budgetFaker.Generate();
        child2Budget.Category = "Service & Parts";
        child2Budget.Limit = 200;
        child2Budget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(child2Budget);

        var child3Budget = budgetFaker.Generate();
        child3Budget.Category = "Paycheck";
        child3Budget.Limit = 300;
        child3Budget.Month = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

        helper.UserDataContext.Budgets.Add(child3Budget);

        helper.UserDataContext.SaveChanges();

        var budget = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 500,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(5);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Income");
        helper
            .UserDataContext.Budgets.Should()
            .Contain(b => b.Limit == child1Budget.Limit + budget.Limit);
    }

    [Fact]
    public async Task CreateBudgetAsync_AutoManageParents_WhenParentCategoryExistsAndHasLimitSmallerThanNewBudget_ShouldUpdateLimit()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1000;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);
        helper.UserDataContext.SaveChanges();

        var budget = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 200,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        var oldLimit = parentBudget.Limit;

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        var updatedParentBudget = helper.UserDataContext.Budgets.First(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(oldLimit + budget.Limit);
        updatedParentBudget.Month.Should().Be(parentBudget.Month);
        var childBudget = helper.UserDataContext.Budgets.First(b => b.Category == budget.Category);
        childBudget.Limit.Should().Be(budget.Limit);
        childBudget.Month.Should().Be(budget.Month);
    }

    [Fact]
    public async Task CreateBudgetAsync_AutoManageParents_WhenParentCategoryExistsAndHasLimitLargerThanNewBudget_ShouldNotUpdateLimit()
    { // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1000;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);
        helper.UserDataContext.SaveChanges();

        var budget = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 200,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        var updatedParentBudget = helper.UserDataContext.Budgets.First(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(parentBudget.Limit);
        updatedParentBudget.Month.Should().Be(parentBudget.Month);
        var childBudget = helper.UserDataContext.Budgets.First(b => b.Category == budget.Category);
        childBudget.Limit.Should().Be(budget.Limit);
        childBudget.Month.Should().Be(budget.Month);
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenCategoryIsEmpty_ShouldThrowBudgetCreateCategoryNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget = new BudgetCreateRequest
        {
            Category = "",
            Limit = 100,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                "BudgetCreateCompletedWithErrorsError [BudgetCreateCategoryNotFoundError []]"
            );
    }

    [Fact]
    public async Task CreateBudgetsAsync_WithMixOfValidAndInvalid_ShouldThrowBudgetCreateCategoryNotFoundErrorWithPartialCreation()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validBudget1 = new BudgetCreateRequest
        {
            Category = "Paycheck",
            Limit = 100,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        var invalidBudget = new BudgetCreateRequest
        {
            Category = null!,
            Limit = 200,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        var validBudget2 = new BudgetCreateRequest
        {
            Category = "Bonus",
            Limit = 300,
            Month = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(
                helper.demoUser.Id,
                [validBudget1, invalidBudget, validBudget2]
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage(
                "BudgetCreateCompletedWithErrorsError [BudgetCreateCategoryNotFoundError []]"
            );
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        var firstBudget = helper.UserDataContext.Budgets.FirstOrDefault(b =>
            b.Category == validBudget1.Category
        );
        firstBudget.Should().NotBeNull();
        firstBudget.Limit.Should().Be(validBudget1.Limit);
        firstBudget.Month.Should().Be(validBudget1.Month);
        var secondBudget = helper.UserDataContext.Budgets.FirstOrDefault(b =>
            b.Category == validBudget2.Category
        );
        secondBudget.Should().NotBeNull();
        secondBudget.Limit.Should().Be(validBudget2.Limit);
        secondBudget.Month.Should().Be(validBudget2.Month);
    }
    #endregion

    #region ReadBudgetsAsync
    [Fact]
    public async Task ReadBudgetsAsync_WhenValidData_ShouldReturnBudgets()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budgets = budgetFaker.Generate(20);

        helper.UserDataContext.Budgets.AddRange(budgets);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await budgetService.ReadBudgetsAsync(
            helper.demoUser.Id,
            DateOnly.FromDateTime(DateTime.Now)
        );

        // Assert
        result
            .Should()
            .HaveCount(
                budgets.Count(b =>
                    b.Month.Month == DateTime.Now.Month && b.Month.Year == DateTime.Now.Year
                )
            );
        result
            .Should()
            .BeEquivalentTo(
                budgets
                    .Where(b => b.Month.Month == DateTime.Now.Month)
                    .Select(b => new BudgetResponse(b))
            );
    }
    #endregion

    #region UpdateBudgetAsync
    [Fact]
    public async Task UpdateBudgetAsync_WhenValidData_ShouldUpdateBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingBudget = new Budget
        {
            ID = Guid.NewGuid(),
            Category = "Paycheck",
            Limit = 1000,
            Month = DateOnly.FromDateTime(DateTime.Today),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.Budgets.Add(existingBudget);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = new BudgetUpdateRequest
        {
            ID = existingBudget.ID,
            Limit = existingBudget.Limit + 100,
        };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        var updatedBudgetFromDb = helper.UserDataContext.Budgets.Single(b =>
            b.ID == existingBudget.ID
        );
        updatedBudgetFromDb.Should().BeEquivalentTo(updatedBudget);
    }

    [Fact]
    public async Task UpdateBudgetAsync_InvalidBudgetID_ThrowsBudgetNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budget = budgetFaker.Generate();

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = new BudgetUpdateRequest
        {
            ID = Guid.NewGuid(),
            Limit = budget.Limit + 100,
        };

        // Act
        Func<Task> act = async () =>
            await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BudgetNotFoundError");
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildBudgetLargerThanParent_ShouldUpdateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var parentLimit = 1000;

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = parentLimit;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);

        var otherChildBudgetLimit = 300;

        var otherChildBudget = budgetFaker.Generate();
        otherChildBudget.Category = "Bonus";
        otherChildBudget.Limit = otherChildBudgetLimit;
        otherChildBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(otherChildBudget);
        helper.UserDataContext.SaveChanges();

        var newChildLimit = 3000;
        var budget = new BudgetUpdateRequest { ID = childBudget.ID, Limit = newChildLimit };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(newChildLimit + otherChildBudgetLimit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildBudgetLessThanParent_ShouldNotUpdateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var parentLimit = 1000;

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = parentLimit;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var budget = new BudgetUpdateRequest { ID = childBudget.ID, Limit = 100 };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(parentLimit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildHasNoParent_ShouldCreateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var newBudgetLimit = 3000;
        var budget = new BudgetUpdateRequest { ID = childBudget.ID, Limit = newBudgetLimit };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        helper.UserDataContext.Budgets.Any((b) => b.Category == "Income").Should().BeTrue();
        helper
            .UserDataContext.Budgets.Single((b) => b.Category == "Income")
            .Limit.Should()
            .Be(newBudgetLimit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenCategoryHasNoResolvedParent_ShouldNotCreateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Category That Does Not Exist";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var budget = new BudgetUpdateRequest { ID = childBudget.ID, Limit = 300 };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
        helper.UserDataContext.Budgets.Should().ContainSingle(b => b.ID == childBudget.ID);
        helper.UserDataContext.Budgets.Should().NotContain(b => b.Category == string.Empty);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedParentLessThanChildren_ShouldThrowBudgetUpdateParentLimitError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1000;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);
        var childBudget = budgetFaker.Generate();

        childBudget.UserID = helper.demoUser.Id;
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var budget = new BudgetUpdateRequest { ID = parentBudget.ID, Limit = 100 };

        // Act
        Func<Task> act = async () =>
            await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BudgetUpdateParentLimitError");
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenChildHasMultipleSiblings_ShouldUpdateParentCorrectly()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1500;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget1 = budgetFaker.Generate();
        childBudget1.Category = "Paycheck";
        childBudget1.Limit = 1000;
        childBudget1.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget1);

        var childBudget2 = budgetFaker.Generate();
        childBudget2.Category = "Bonus";
        childBudget2.Limit = 500;
        childBudget2.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget2);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = new BudgetUpdateRequest
        {
            ID = childBudget1.ID,
            Limit = childBudget1.Limit + 1000,
        };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(2500);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WithNegativeLimit_ShouldUpdateToNegativeLimit()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingBudget = new Budget
        {
            ID = Guid.NewGuid(),
            Category = "Auto & Transport",
            Limit = 1000,
            Month = DateOnly.FromDateTime(DateTime.Today),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.Budgets.Add(existingBudget);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = new BudgetUpdateRequest { ID = existingBudget.ID, Limit = -50 };

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        helper.UserDataContext.Budgets.Single().Limit.Should().Be(-50);
    }
    #endregion

    #region DeleteBudgetAsync
    [Fact]
    public async Task DeleteBudgetAsync_WhenValidData_ShouldDeleteBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budget = budgetFaker.Generate();

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        // Act
        await budgetService.DeleteBudgetAsync(helper.demoUser.Id, budget.ID);

        // Assert
        helper.UserDataContext.Budgets.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBudgetAsync_InvalidBudgetID_ThrowsBudgetNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budget = budgetFaker.Generate();

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await budgetService.DeleteBudgetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BudgetNotFoundError");
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenParentBudgetHasChildren_ShouldDeleteChildren()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1000;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);
        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget);

        var childBudget2 = budgetFaker.Generate();
        childBudget2.Category = "Paycheck";
        childBudget2.Limit = 200;
        childBudget2.Month = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

        helper.UserDataContext.Budgets.Add(childBudget2);
        helper.UserDataContext.SaveChanges();

        // Act
        await budgetService.DeleteBudgetAsync(helper.demoUser.Id, parentBudget.ID);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenDeletingChildWithSiblings_ShouldNotAffectParent()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var parentBudget = budgetFaker.Generate();
        parentBudget.Category = "Income";
        parentBudget.Limit = 1500;
        parentBudget.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget1 = budgetFaker.Generate();
        childBudget1.Category = "Paycheck";
        childBudget1.Limit = 1000;
        childBudget1.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget1);

        var childBudget2 = budgetFaker.Generate();
        childBudget2.Category = "Bonus";
        childBudget2.Limit = 500;
        childBudget2.Month = DateOnly.FromDateTime(DateTime.Today);

        helper.UserDataContext.Budgets.Add(childBudget2);
        helper.UserDataContext.SaveChanges();

        // Act
        await budgetService.DeleteBudgetAsync(helper.demoUser.Id, childBudget1.ID);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper
            .UserDataContext.Budgets.Should()
            .Contain(b => b.ID == parentBudget.ID && b.Limit == parentBudget.Limit);
        helper.UserDataContext.Budgets.Should().Contain(b => b.ID == childBudget2.ID);
        helper.UserDataContext.Budgets.Should().NotContain(b => b.ID == childBudget1.ID);
    }
    #endregion
}
