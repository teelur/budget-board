using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
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
            .RuleFor(b => b.Category, f => f.Finance.AccountName())
            .RuleFor(b => b.Limit, f => f.Finance.Amount());

    private readonly Faker<BudgetUpdateRequest> _budgetUpdateRequestFaker =
        new Faker<BudgetUpdateRequest>()
            .RuleFor(b => b.ID, f => Guid.NewGuid())
            .RuleFor(b => b.Limit, f => f.Finance.Amount());

    [Fact]
    public async Task CreateBudgetsAsync_InvalidUserGuid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budget = _budgetCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(Guid.NewGuid(), [budget]);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenValidData_ShouldCreateBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budget = _budgetCreateRequestFaker.Generate();

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().ContainSingle();
        helper.UserDataContext.Budgets.Single().Should().BeEquivalentTo(budget);
    }

    [Fact]
    public async Task CreateBudgetsAsync_DuplicateCategory_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var existingBudget = budgetFaker.Generate();
        existingBudget.UserID = helper.demoUser.Id;
        existingBudget.Date = DateTime.Today;
        existingBudget.Category = "Paycheck";
        existingBudget.Limit = 1000;

        helper.UserDataContext.Budgets.Add(existingBudget);
        helper.UserDataContext.SaveChanges();

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Date = DateTime.Today;
        budget.Category = "Paycheck";

        // Act
        Func<Task> act = async () =>
            await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);
        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The budget(s) you are trying to create already exist.");
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenCreateChild_ShouldCreateParent()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budget = _budgetCreateRequestFaker.Generate();
        budget.Category = "Paycheck";

        // Act
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

        // Assert
        var parentBudget = new Budget
        {
            Date = budget.Date,
            Category = "Income",
            Limit = budget.Limit,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == "Income");
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Date == budget.Date);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Limit == budget.Limit);
        helper.UserDataContext.Budgets.Should().Contain(b => b.UserID == helper.demoUser.Id);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Date == parentBudget.Date);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == parentBudget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Limit == parentBudget.Limit);
        helper.UserDataContext.Budgets.Should().Contain(b => b.UserID == parentBudget.UserID);
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenCreateChildAndChildAlreadyExists_ShouldCreateParentWithSumOfLimits()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var child1Budget = budgetFaker.Generate();
        child1Budget.UserID = helper.demoUser.Id;
        child1Budget.Category = "Bonus";
        child1Budget.Limit = 1000;
        child1Budget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(child1Budget);

        var child2Budget = budgetFaker.Generate();
        child2Budget.UserID = helper.demoUser.Id;
        child2Budget.Category = "Service & Parts";
        child2Budget.Limit = 200;
        child2Budget.Date = DateTime.Today;

        helper.UserDataContext.Budgets.Add(child2Budget);

        var child3Budget = budgetFaker.Generate();
        child3Budget.UserID = helper.demoUser.Id;
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
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

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
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

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
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == parentBudget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Limit == parentBudget.Limit);
    }

    [Fact]
    public async Task CreateBudgetAsync_WhenIsCopy_ShouldNotAddParent()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget], true);

        // Assert
        helper.UserDataContext.Budgets.Should().HaveCount(2);
        helper.UserDataContext.Budgets.Should().Contain(b => b.Category == budget.Category);
        helper
            .UserDataContext.Budgets.Single(b => b.Category == parentBudget.Category)
            .Limit.Should()
            .Be(1000);
    }

    [Fact]
    public async Task ReadBudgetsAsync_WhenValidData_ShouldReturnBudgets()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var budgets = budgetFaker.Generate(20);
        budgets.ForEach(b => b.UserID = helper.demoUser.Id);

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
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

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
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

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
            .WithMessage("The budget you are trying to update does not exist.");
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildBudgetLargerThanParent_ShouldUpdateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        budget.ID = childBudget.ID;
        budget.Limit = 3000;

        var parentOldLimit = parentBudget.Limit;

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(budget.Limit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildBudgetLessThanParent_ShouldNotUpdateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        budget.ID = childBudget.ID;
        budget.Limit = 100;

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(parentBudget.Limit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedChildHasNoParent_ShouldCreateParentBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
        budget.ID = childBudget.ID;
        budget.Limit = 3000;

        // Act
        await budgetService.UpdateBudgetAsync(helper.demoUser.Id, budget);

        // Assert
        var updatedParentBudget = helper.UserDataContext.Budgets.Single(b =>
            b.Category == parentBudget.Category
        );
        updatedParentBudget.Limit.Should().Be(budget.Limit);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenUpdatedParentLessThanChildren_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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
            .WithMessage("The parent budget cannot be less than the sum of its children.");
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenValidData_ShouldDeleteBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

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
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await budgetService.DeleteBudgetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The budget you are trying to delete does not exist.");
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenParentBudgetHasChildren_ShouldDeleteChildren()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(
            Mock.Of<ILogger<IBudgetService>>(),
            helper.UserDataContext
        );

        var budgetFaker = new BudgetFaker();
        var parentBudget = budgetFaker.Generate();
        parentBudget.UserID = helper.demoUser.Id;
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

        var childBudget2 = budgetFaker.Generate();
        childBudget2.UserID = helper.demoUser.Id;
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
