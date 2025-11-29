using Bogus;
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
    private readonly Faker<BudgetCreateRequest> _budgetCreateRequestFaker =
        new Faker<BudgetCreateRequest>()
            .RuleFor(b => b.Date, f => f.Date.Past())
            .RuleFor(
                b => b.Category,
                (f, b) =>
                    f.PickRandom(
                        TransactionCategoriesConstants.DefaultTransactionCategories.Select(tc =>
                            tc.Value
                        )
                    )
            )
            .RuleFor(b => b.Limit, f => f.Finance.Amount());

    private readonly Faker<BudgetUpdateRequest> _budgetUpdateRequestFaker =
        new Faker<BudgetUpdateRequest>()
            .RuleFor(b => b.ID, f => Guid.NewGuid())
            .RuleFor(b => b.Limit, f => f.Finance.Amount());

    [Fact]
    public async Task CreateBudgetsWithParentsAsync_WhenValidData_ShouldCreateBudgetsWithParents()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var today = DateTime.Today;
        var budgets = new List<BudgetCreateRequest>();
        var newBudget1 = _budgetCreateRequestFaker.Generate();
        newBudget1.Category = "Paycheck";
        newBudget1.Date = today;
        budgets.Add(newBudget1);
        var newBudget2 = _budgetCreateRequestFaker.Generate();
        newBudget2.Category = "Bonus";
        newBudget2.Date = today;
        budgets.Add(newBudget2);
        var newBudget3 = _budgetCreateRequestFaker.Generate();
        newBudget3.Category = "Service & Parts";
        newBudget3.Date = today;
        budgets.Add(newBudget3);

        // Act
        await budgetService.CreateBudgetsWithParentsAsync(helper.demoUser.Id, budgets);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(5);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Income");
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Paycheck");
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Bonus");
    }

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

        var budgets = _budgetCreateRequestFaker.Generate(5);

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, budgets);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(5);
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

        var budget = _budgetCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(Guid.NewGuid(), [budget]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateBudgetsWithParentsAsync_WhenCreateChildAndChildAlreadyExists_ShouldCreateParentWithSumOfLimits()
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
        child1Budget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(child1Budget);

        var child2Budget = budgetFaker.Generate();
        child2Budget.Category = "Service & Parts";
        child2Budget.Limit = 200;
        child2Budget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(child2Budget);

        var child3Budget = budgetFaker.Generate();
        child3Budget.Category = "Paycheck";
        child3Budget.Limit = 300;
        child3Budget.Date = DateTime.Today.AddMonths(-1);

        helper.UserDataContext.Budgets.Add(child3Budget);

        helper.UserDataContext.SaveChanges();

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Category = "Paycheck";
        budget.Limit = 200;
        budget.Date = DateTime.Today;

        // Act
        await budgetService.CreateBudgetsWithParentsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(5);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Income");
        helper
            .UserDataContext.Budgets.Should()
            .Contain(b => b.Limit == child1Budget.Limit + budget.Limit);
    }

    [Fact]
    public async Task CreateBudgetAsync_WhenParentCategoryExistsAndHasLimitSmallerThanNewBudget_ShouldUpdateLimit()
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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);
        helper.UserDataContext.SaveChanges();

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Category = "Paycheck";
        budget.Limit = 200;
        budget.Date = DateTime.Today;

        var oldLimit = parentBudget.Limit;

        // Act
        await budgetService.CreateBudgetsWithParentsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == parentBudget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Limit == oldLimit + budget.Limit);
    }

    [Fact]
    public async Task CreateBudgetAsync_WhenParentCategoryExistsAndHasLimitLargerThanNewBudget_ShouldNotUpdateLimit()
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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);
        helper.UserDataContext.SaveChanges();

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Category = "Paycheck";
        budget.Limit = 200;
        budget.Date = DateTime.Today;

        // Act
        await budgetService.CreateBudgetsWithParentsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == parentBudget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Limit == parentBudget.Limit);
    }

    [Fact]
    public async Task CreateBudgetAsync_WhenAddChildren_ShouldNotAddParent()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Category = "Paycheck";
        budget.Limit = 200;
        budget.Date = DateTime.Today;

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
    }

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
        var result = await budgetService.ReadBudgetsAsync(helper.demoUser.Id, DateTime.Now);

        // Assert
        result
            .Should()
            .HaveCount(
                budgets
                    .Where(b =>
                        b.Date.Month == DateTime.Now.Month && b.Date.Year == DateTime.Now.Year
                    )
                    .Count()
            );
        result
            .Should()
            .BeEquivalentTo(
                budgets
                    .Where(b => b.Date.Month == DateTime.Now.Month)
                    .Select(b => new BudgetResponse(b))
            );
    }

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

        var budgetFaker = new BudgetFaker(helper.demoUser.Id);
        var budget = budgetFaker.Generate();

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = _budgetUpdateRequestFaker.Generate();
        updatedBudget.ID = budget.ID;

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        helper.UserDataContext.Budgets.Single().Should().BeEquivalentTo(updatedBudget);
    }

    [Fact]
    public async Task UpdateBudgetAsync_InvalidBudgetID_ThrowsException()
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

        var updatedBudget = _budgetUpdateRequestFaker.Generate();
        updatedBudget.ID = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BudgetUpdateNotFoundError");
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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(childBudget);

        var otherChildBudgetLimit = 300;

        var otherChildBudget = budgetFaker.Generate();
        otherChildBudget.Category = "Bonus";
        otherChildBudget.Limit = otherChildBudgetLimit;
        otherChildBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(otherChildBudget);
        helper.UserDataContext.SaveChanges();

        var newChildLimit = 3000;

        var budget = _budgetUpdateRequestFaker.Generate();
        budget.ID = childBudget.ID;
        budget.Limit = newChildLimit;

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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);

        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var budget = _budgetUpdateRequestFaker.Generate();
        budget.ID = childBudget.ID;
        budget.Limit = 100;

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
        childBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var newBudgetLimit = 3000;

        var budget = _budgetUpdateRequestFaker.Generate();
        budget.ID = childBudget.ID;
        budget.Limit = newBudgetLimit;

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
    public async Task UpdateBudgetAsync_WhenUpdatedParentLessThanChildren_ShouldThrowError()
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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);
        var childBudget = budgetFaker.Generate();

        childBudget.UserID = helper.demoUser.Id;
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(childBudget);
        helper.UserDataContext.SaveChanges();

        var budget = _budgetUpdateRequestFaker.Generate();
        budget.ID = parentBudget.ID;
        budget.Limit = 100;

        // Act
        Func<Task> act = async () =>
            await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BudgetUpdateParentLimitError");
    }

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
    public async Task DeleteBudgetAsync_InvalidBudgetID_ThrowsException()
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
            .WithMessage("BudgetDeleteNotFoundError");
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
        parentBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(parentBudget);
        var childBudget = budgetFaker.Generate();
        childBudget.Category = "Paycheck";
        childBudget.Limit = 200;
        childBudget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(childBudget);

        var childBudget2 = budgetFaker.Generate();
        childBudget2.Category = "Paycheck";
        childBudget2.Limit = 200;
        childBudget2.Date = DateTime.Today.AddMonths(-1);

        helper.UserDataContext.Budgets.Add(childBudget2);
        helper.UserDataContext.SaveChanges();

        // Act
        await budgetService.DeleteBudgetAsync(helper.demoUser.Id, parentBudget.ID);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(1);
    }
}
