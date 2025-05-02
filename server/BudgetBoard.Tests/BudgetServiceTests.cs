using Bogus;
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
    private readonly Faker<BudgetCreateRequest> _budgetCreateRequestFaker = new Faker<BudgetCreateRequest>()
        .RuleFor(b => b.Date, f => f.Date.Past())
        .RuleFor(b => b.Category, f => f.Finance.AccountName())
        .RuleFor(b => b.Limit, f => f.Finance.Amount());

    private readonly Faker<BudgetUpdateRequest> _budgetUpdateRequestFaker = new Faker<BudgetUpdateRequest>()
        .RuleFor(b => b.ID, f => Guid.NewGuid())
        .RuleFor(b => b.Limit, f => f.Finance.Amount());

    [Fact]
    public async Task CreateBudgetsAsync_InvalidUserGuid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

        var budget = _budgetCreateRequestFaker.Generate();


        // Act
        Func<Task> act = async () => await budgetService.CreateBudgetsAsync(Guid.NewGuid(), [budget]);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateBudgetsAsync_WhenValidData_ShouldCreateBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

        var budget = _budgetCreateRequestFaker.Generate();
        await helper.UserDataContext.SaveChangesAsync();

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
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

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
        Func<Task> act = async () => await budgetService.CreateBudgetsAsync(helper.demoUser.Id, [budget]);
        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The budget(s) you are trying to create already exist.");
    }

    [Fact]
    public async Task ReadBudgetsAsync_WhenValidData_ShouldReturnBudgets()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

        var budgetFaker = new BudgetFaker();
        var budgets = budgetFaker.Generate(20);
        budgets.ForEach(b => b.UserID = helper.demoUser.Id);

        helper.UserDataContext.Budgets.AddRange(budgets);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await budgetService.ReadBudgetsAsync(helper.demoUser.Id, DateTime.Now);

        // Assert
        result.Should().HaveCount(budgets.Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Year == DateTime.Now.Year).Count());
        result.Should().BeEquivalentTo(budgets.Where(b => b.Date.Month == DateTime.Now.Month).Select(b => new BudgetResponse(b)));
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenValidData_ShouldUpdateBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

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
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        var updatedBudget = _budgetUpdateRequestFaker.Generate();
        updatedBudget.ID = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await budgetService.UpdateBudgetAsync(helper.demoUser.Id, updatedBudget);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The budget you are trying to update does not exist.");
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenValidData_ShouldDeleteBudget()
    {
        // Arrange
        var helper = new TestHelper();
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);
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
        var budgetService = new BudgetService(Mock.Of<ILogger<IBudgetService>>(), helper.UserDataContext);

        var budgetFaker = new BudgetFaker();
        var budget = budgetFaker.Generate();
        budget.UserID = helper.demoUser.Id;

        helper.UserDataContext.Budgets.Add(budget);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () => await budgetService.DeleteBudgetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The budget you are trying to delete does not exist.");
    }
}
