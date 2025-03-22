﻿using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class GoalServiceTests
{
    private readonly Faker<GoalCreateRequest> _goalCreateRequestFaker = new Faker<GoalCreateRequest>()
        .RuleFor(g => g.Name, f => f.Lorem.Word())
        .RuleFor(g => g.CompleteDate, f => f.Date.Future())
        .RuleFor(g => g.Amount, f => f.Finance.Amount())
        .RuleFor(g => g.InitialAmount, f => f.Finance.Amount())
        .RuleFor(g => g.MonthlyContribution, f => f.Finance.Amount());

    private readonly Faker<GoalUpdateRequest> _goalUpdateRequestFaker = new Faker<GoalUpdateRequest>()
        .RuleFor(g => g.Amount, f => f.Finance.Amount())
        .RuleFor(g => g.IsMonthlyContributionEditable, f => true)
        .RuleFor(g => g.MonthlyContribution, f => f.Finance.Amount())
        .RuleFor(g => g.IsCompleteDateEditable, f => true)
        .RuleFor(g => g.CompleteDate, f => f.Date.Future());

    [Fact]
    public async Task CreateGoalAsync_InvalidUserId_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var goal = _goalCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(Guid.NewGuid(), goal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenValidData_ShouldCreateGoal()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [.. accounts.Select(a => a.ID)];

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().Should().BeEquivalentTo(goal, options => options
        .Excluding(g => g.AccountIds));
        helper.UserDataContext.Goals.Single().Accounts.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public async Task CreateGoalAsync_InvalidAccountId_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [Guid.NewGuid()];

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The account you are trying to use does not exist.");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenInitialAmountNotSet_ShouldSetToAccountRunningBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);

        var balanceFaker = new BalanceFaker();
        accounts.ForEach(a =>
        {
            a.UserID = helper.demoUser.Id;
            var balance = balanceFaker.Generate();
            balance.AccountID = a.ID;
            a.Balances.Add(balance);

            helper.UserDataContext.Balances.Add(balance);
        });

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [.. accounts.Select(a => a.ID)];
        goal.InitialAmount = null;

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().InitialAmount.Should().Be(accounts.Select(a => a.Balances.Single().Amount).Sum());
    }

    [Fact]
    public async Task CreateGoalAsync_WhenNoMonthlyContributionOrCompleteDate_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [.. accounts.Select(a => a.ID)];
        goal.MonthlyContribution = null;
        goal.CompleteDate = null;

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("A goal must have a monthly contribution or target date.");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenCompleteDateIsInPast_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [.. accounts.Select(a => a.ID)];
        goal.CompleteDate = DateTime.Now.AddMonths(-1);

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("A goal cannot have a target date in the past.");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenNoAccounts_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var goal = _goalCreateRequestFaker.Generate();
        goal.AccountIds = [];

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("A goal must be associated with at least one account.");
    }

    // This test is created with an APR of 48%. The expected values were validated with an online calculator.
    [Theory]
    [InlineData(false, 19)]
    [InlineData(true, 33)]
    public async Task ReadGoalsAsync_WhenNoCompleteDate_ShouldEstimateCompleteDate(bool includeInterest, int monthsToPayoff)
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var balanceFaker = new BalanceFaker();

        var balance0 = balanceFaker.Generate();
        balance0.AccountID = account.ID;
        balance0.Amount = -54080;
        balance0.DateTime = DateTime.Now;

        var balance1 = balanceFaker.Generate();
        balance1.AccountID = account.ID;
        balance1.Amount = -52000;
        balance1.DateTime = DateTime.Now.AddMonths(-1);

        helper.UserDataContext.Balances.Add(balance1);

        var balance2 = balanceFaker.Generate();
        balance2.AccountID = account.ID;
        balance2.Amount = -50000;
        balance2.DateTime = DateTime.Now.AddMonths(-2);

        account.Balances = [balance0, balance1, balance2];

        helper.UserDataContext.Balances.AddRange([balance0, balance1, balance2]);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;

        goal.Accounts = [account];
        goal.CompleteDate = null;
        goal.Amount = 0;
        goal.InitialAmount = -60000;
        goal.MonthlyContribution = 3000;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, includeInterest);

        // Assert
        result.Single().CompleteDate.Should().Be(new DateTime(DateTime.Now.AddMonths(monthsToPayoff).Year, DateTime.Now.AddMonths(monthsToPayoff).Month, 1));
    }

    // This test is created with an APR of 48%. The expected values were validated with an online calculator.
    [Theory]
    [InlineData(false, 901)]
    [InlineData(true, 2390)]
    public async Task ReadGoalsAsync_WhenNoMonthlyContribution_ShouldEstimateMonthlyContribution(bool includeInterest, decimal monthlyContribution)
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var balanceFaker = new BalanceFaker();

        var balance0 = balanceFaker.Generate();
        balance0.AccountID = account.ID;
        balance0.Amount = -54080;
        balance0.DateTime = DateTime.Now;

        var balance1 = balanceFaker.Generate();
        balance1.AccountID = account.ID;
        balance1.Amount = -52000;
        balance1.DateTime = DateTime.Now.AddMonths(-1);

        helper.UserDataContext.Balances.Add(balance1);

        var balance2 = balanceFaker.Generate();
        balance2.AccountID = account.ID;
        balance2.Amount = -50000;
        balance2.DateTime = DateTime.Now.AddMonths(-2);

        account.Balances = [balance0, balance1, balance2];

        helper.UserDataContext.Balances.AddRange([balance0, balance1, balance2]);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;

        goal.Accounts = [account];
        goal.CompleteDate = DateTime.Now.AddYears(5);
        goal.Amount = 0;
        goal.InitialAmount = -60000;
        goal.MonthlyContribution = null;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, includeInterest);

        // Assert
        result.Single().MonthlyContribution.Should().BeApproximately(monthlyContribution, 1);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenValidData_ShouldUpdateData()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = _goalUpdateRequestFaker.Generate();
        updatedGoal.ID = goal.ID;

        // Act
        await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().Should().BeEquivalentTo(updatedGoal, options => options
        .Excluding(g => g.IsCompleteDateEditable)
        .Excluding(g => g.IsMonthlyContributionEditable));
    }

    [Fact]
    public async Task UpdateGoalAsync_InvalidGoalId_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var updatedGoal = _goalUpdateRequestFaker.Generate();

        // Act
        Func<Task> act = async () => await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The goal you are trying to update does not exist.");
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenIsMonthlyContributionEditableFalse_ShouldNotUpdateMonthlyContribution()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = _goalUpdateRequestFaker.Generate();
        updatedGoal.ID = goal.ID;
        updatedGoal.IsMonthlyContributionEditable = false;

        // Act
        await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().MonthlyContribution.Should().Be(goal.MonthlyContribution);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenIsCompleteDateEditableFalse_ShouldNotUpdateCompleteDate()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = _goalUpdateRequestFaker.Generate();
        updatedGoal.ID = goal.ID;
        updatedGoal.IsCompleteDateEditable = false;

        // Act
        await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().CompleteDate.Should().Be(goal.CompleteDate);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenCompleteDateSetToPast_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = _goalUpdateRequestFaker.Generate();
        updatedGoal.ID = goal.ID;
        updatedGoal.CompleteDate = DateTime.Now.AddMonths(-1);
        updatedGoal.IsCompleteDateEditable = true;

        // Act
        Func<Task> act = async () => await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("A goal cannot have a target date in the past.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(null)]
    public async Task UpdateGoalAsync_WhenMonthlyContributionInvalid_ShouldThrowError(int? monthlyContribution)
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = _goalUpdateRequestFaker.Generate();
        updatedGoal.ID = goal.ID;
        updatedGoal.MonthlyContribution = monthlyContribution;
        updatedGoal.IsMonthlyContributionEditable = true;

        // Act
        Func<Task> act = async () => await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("A goal must have a monthly contribution greater than 0.");
    }

    [Fact]
    public async Task DeleteGoalAsync_WhenValidData_ShouldDeleteGoal()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(5);
        accounts.ForEach(a => a.UserID = helper.demoUser.Id);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker();
        var goal = goalFaker.Generate();
        goal.UserID = helper.demoUser.Id;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        await goalService.DeleteGoalAsync(helper.demoUser.Id, goal.ID);

        // Assert
        helper.UserDataContext.Goals.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteGoalAsync_WhenInvalidGoal_ShouldThrowError()
    {   // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(Mock.Of<ILogger<IGoalService>>(), helper.UserDataContext);

        // Act
        Func<Task> act = async () => await goalService.DeleteGoalAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>().WithMessage("The goal you are trying to delete does not exist.");
    }

}
