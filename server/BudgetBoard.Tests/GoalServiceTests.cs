using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class GoalServiceTests
{
    #region CreateGoalAsync
    [Fact]
    public async Task CreateGoalAsync_WhenApplyExistingBalanceTowardsGoalIsFalse_ShouldCreateGoalWithInitialAmountAsSumOfAccountBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        var today = fakeNowProvider.Today;
        var aMonthAgo = today.AddMonths(-1);
        var expectedBalance = 0.0M;
        foreach (var account in accounts)
        {
            var balanceFaker = new BalanceFaker([account.ID]);
            var latestBalance = balanceFaker.Generate();
            latestBalance.AccountID = account.ID;
            latestBalance.Date = today;
            expectedBalance += latestBalance.Amount;

            var olderBalance = balanceFaker.Generate();
            olderBalance.AccountID = account.ID;
            olderBalance.Date = aMonthAgo;
            account.Balances.Add(latestBalance);
            account.Balances.Add(olderBalance);

            helper.UserDataContext.Balances.Add(latestBalance);
            helper.UserDataContext.Balances.Add(olderBalance);
        }

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = false,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        var addedGoal = helper.UserDataContext.Goals.Single();
        addedGoal.Name.Should().Be(goal.Name);
        addedGoal.CompleteDate.Should().Be(goal.CompleteDate);
        addedGoal.Amount.Should().Be(goal.Amount);
        addedGoal.InitialAmount.Should().Be(expectedBalance);
        addedGoal.MonthlyContribution.Should().Be(goal.MonthlyContribution);
        addedGoal.Completed.Should().BeNull();
        addedGoal.Accounts.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public async Task CreateGoalAsync_WhenApplyExistingBalanceTowardsGoalIsTrue_ShouldCreateGoalWithInitialAmountAsZero()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        foreach (var account in accounts)
        {
            var balanceFaker = new BalanceFaker([account.ID]);
            var balance = balanceFaker.Generate();
            balance.AccountID = account.ID;
            account.Balances.Add(balance);

            helper.UserDataContext.Balances.Add(balance);
        }

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        var addedGoal = helper.UserDataContext.Goals.Single();
        addedGoal.Name.Should().Be(goal.Name);
        addedGoal.CompleteDate.Should().Be(goal.CompleteDate);
        addedGoal.Amount.Should().Be(goal.Amount);
        addedGoal.InitialAmount.Should().Be(0);
        addedGoal.MonthlyContribution.Should().Be(goal.MonthlyContribution);
        addedGoal.Completed.Should().BeNull();
        addedGoal.Accounts.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public async Task CreateGoalAsync_WhenAccountHasNoBalance_ShouldDefaultToZero()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = false,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        var addedGoal = helper.UserDataContext.Goals.Single();
        addedGoal.InitialAmount.Should().Be(0);
    }

    [Fact]
    public async Task CreateGoalAsync_WhenNullCompleteDate_ShouldUseMonthlyContribution()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = null,
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        var addedGoal = helper.UserDataContext.Goals.Single();
        addedGoal.MonthlyContribution.Should().Be(goal.MonthlyContribution);
        addedGoal.CompleteDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateGoalAsync_InvalidUserId_ThrowsGoalCreateInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [Guid.NewGuid()],
        };

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(Guid.NewGuid(), goal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenNoMonthlyContributionOrCompleteDate_ShouldThrowGoalCreateMissingContributionOrDateError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = null,
            CompleteDate = null,
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalCreateMissingContributionOrDateError");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenCompleteDateIsInPast_ShouldThrowGoalCreateInPastError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock(DateTime.Today.AddDays(-1));
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddDays(-1),
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [.. accounts.Select(a => a.ID)],
        };

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalCreateInPastError");
    }

    [Fact]
    public async Task CreateGoalAsync_WhenNoAccounts_ShouldThrowGoalCreateNoAccountsError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [],
        };

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalCreateNoAccountsError");
    }

    [Fact]
    public async Task CreateGoalAsync_InvalidAccountId_ThrowsGoalCreateInvalidAccountError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var goal = new GoalCreateRequest
        {
            Name = "Test Goal",
            Amount = 1000,
            MonthlyContribution = 100,
            CompleteDate = fakeNowProvider.Today.AddMonths(6),
            ApplyExistingBalanceTowardsGoal = true,
            AccountIds = [Guid.NewGuid()],
        };

        // Act
        Func<Task> act = async () => await goalService.CreateGoalAsync(helper.demoUser.Id, goal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalCreateInvalidAccountError");
    }
    #endregion

    #region ReadGoalsAsync
    [Fact]
    public async Task ReadGoalsAsync_WhenValidData_ShouldReturnGoal()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.CompleteDate = fakeNowProvider.Today.AddMonths(6);
        goal.MonthlyContribution = null;
        goal.Accounts = accounts;
        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        var readGoal = result.Single();
        readGoal.ID.Should().Be(goal.ID);
        readGoal.Name.Should().Be(goal.Name);
        readGoal.CompleteDate.Should().Be(goal.CompleteDate);
        readGoal.IsCompleteDateEditable.Should().Be(true);
        readGoal.Amount.Should().Be(goal.Amount);
        readGoal.InitialAmount.Should().Be(goal.InitialAmount);
        readGoal.MonthlyContribution.Should().NotBe(0);
        readGoal.IsMonthlyContributionEditable.Should().Be(false);
        readGoal.InterestRate.Should().Be(0);
        readGoal.Completed.Should().Be(goal.Completed);
        readGoal.Accounts.Should().BeEquivalentTo(accounts.Select(a => new AccountResponse(a)));
        readGoal.UserID.Should().Be(goal.UserID);
    }

    // This test is created with an APR of 48%. The expected values were validated with an online calculator.
    [Theory]
    [InlineData(-54080, -60000, 0, false, 19)]
    [InlineData(-54080, -60000, 0, true, 33)]
    [InlineData(32000, 0, 600000, false, 190)]
    [InlineData(32000, 0, 600000, true, 47)]
    public async Task ReadGoalsAsync_WhenNoCompleteDate_ShouldEstimateCompleteDate(
        int balance,
        int goalInitialAmount,
        int goalTargetAmount,
        bool includeInterest,
        int monthsToPayoff
    )
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InterestRate = 0.48M;

        var balanceFaker = new BalanceFaker([account.ID]);

        var balance0 = balanceFaker.Generate();
        balance0.Amount = balance;

        account.Balances = [balance0];

        helper.UserDataContext.Balances.Add(balance0);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = null;
        goal.Amount = goalTargetAmount;
        goal.InitialAmount = goalInitialAmount;
        goal.MonthlyContribution = 3000;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, includeInterest);

        // Assert
        result
            .Single()
            .CompleteDate.Should()
            .Be(
                new DateOnly(
                    fakeNowProvider.Today.AddMonths(monthsToPayoff).Year,
                    fakeNowProvider.Today.AddMonths(monthsToPayoff).Month,
                    1
                )
            );
    }

    // This test is created with an APR of 48%. The expected values were validated with an online calculator.
    [Theory]
    [InlineData(-54080, -60000, 0, false, 901)]
    [InlineData(-54080, -60000, 0, true, 2390)]
    [InlineData(32000, 0, 600000, false, 9466)]
    [InlineData(32000, 0, 600000, true, 1106)]
    public async Task ReadGoalsAsync_WhenNoMonthlyContribution_ShouldEstimateMonthlyContribution(
        int balance,
        int goalInitialAmount,
        int goalTargetAmount,
        bool includeInterest,
        decimal monthlyContribution
    )
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InterestRate = 0.48M;

        var balanceFaker = new BalanceFaker([account.ID]);

        var balance0 = balanceFaker.Generate();
        balance0.Amount = balance;

        account.Balances = [balance0];

        helper.UserDataContext.Balances.Add(balance0);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = fakeNowProvider.Today.AddYears(5);
        goal.Amount = goalTargetAmount;
        goal.InitialAmount = goalInitialAmount;
        goal.MonthlyContribution = null;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, includeInterest);

        // Assert
        result.Single().MonthlyContribution.Should().BeApproximately(monthlyContribution, 1);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenTransactionData_ShouldReturnProgressForMonth()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.AccountID = account.ID;
        transaction.Date = fakeNowProvider.Today;
        transaction.Amount = 3000;

        var otherMonthTransaction = transactionFaker.Generate();
        otherMonthTransaction.AccountID = account.ID;
        otherMonthTransaction.Date = fakeNowProvider.Today.AddMonths(-1);
        otherMonthTransaction.Amount = 2000;

        var otherYearTransaction = transactionFaker.Generate();
        otherYearTransaction.AccountID = account.ID;
        otherYearTransaction.Date = fakeNowProvider.Today.AddYears(-1);
        otherYearTransaction.Amount = 1000;

        account.Transactions.Add(transaction);
        account.Transactions.Add(otherMonthTransaction);
        account.Transactions.Add(otherYearTransaction);

        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.Transactions.Add(otherMonthTransaction);
        helper.UserDataContext.Transactions.Add(otherYearTransaction);

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance0 = balanceFaker.Generate();
        balance0.Amount = 30000;
        balance0.Date = fakeNowProvider.Today;

        account.Balances = [balance0];

        helper.UserDataContext.Balances.Add(balance0);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = null;
        goal.Amount = 50000;
        goal.InitialAmount = 10000;
        goal.MonthlyContribution = 3000;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().MonthlyContributionProgress.Should().BeApproximately(3000, 1);
        result.Single().PercentComplete.Should().BeApproximately(40.0M, 0.01M);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenMonthlyContributionIsNull_ShouldReturnUnixEpochForCompleteDate()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;
        goal.CompleteDate = null;
        goal.MonthlyContribution = null;
        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().CompleteDate.Should().Be(DateOnly.FromDateTime(DateTime.UnixEpoch));
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenAccountsHaveNoBalance_ShouldAssumeBalanceOfZeroForCompleteDateCalculation()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goal = new Goal
        {
            Name = "Test Goal",
            CompleteDate = null,
            MonthlyContribution = 100,
            Amount = 5000,
            InitialAmount = 0,
            Accounts = accounts,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result
            .Single()
            .CompleteDate.Should()
            .Be(
                new DateOnly(fakeNowProvider.Today.Year, fakeNowProvider.Today.Month, 1).AddMonths(
                    50
                )
            );
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenAccountsHaveNoBalance_ShouldAssumeBalanceOfZeroForMonthlyContributionCalculation()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goal = new Goal
        {
            Name = "Test Goal",
            CompleteDate = new DateOnly(
                fakeNowProvider.Today.Year,
                fakeNowProvider.Today.Month,
                1
            ).AddMonths(50),
            MonthlyContribution = null,
            Amount = 5000,
            InitialAmount = 0,
            Accounts = accounts,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().MonthlyContribution.Should().Be(100);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenCompleteDateIsPastDue_ShouldHaveMonthlyContributionOfRestOfLoan()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);

        var balance0 = balanceFaker.Generate();
        balance0.Amount = 30000;

        account.Balances = [balance0];

        helper.UserDataContext.Balances.Add(balance0);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = fakeNowProvider.Today.AddMonths(-1);
        goal.Amount = 50000;
        goal.InitialAmount = 10000;
        goal.MonthlyContribution = null;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().MonthlyContribution.Should().Be(30000);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenNoTransactionsForAccount_ShouldNotCalculateMonthlyContributionProgress()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = fakeNowProvider.Today.AddMonths(20);
        goal.Amount = 50000;
        goal.InitialAmount = 10000;
        goal.MonthlyContribution = 3000;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().MonthlyContributionProgress.Should().Be(0);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenAmountIsZero_ShouldNotReturnProgress()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = fakeNowProvider.Today.AddMonths(20);
        goal.Amount = 0;
        goal.InitialAmount = 10000;
        goal.MonthlyContribution = null;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().PercentComplete.Should().Be(0);
    }

    [Fact]
    public async Task ReadGoalsAsync_WhenPastTarget_ShouldNotReturnAbove100PercentComplete()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance0 = balanceFaker.Generate();
        balance0.Amount = 30000;

        account.Balances = [balance0];

        helper.UserDataContext.Balances.Add(balance0);
        helper.UserDataContext.Accounts.Add(account);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();

        goal.Accounts = [account];
        goal.CompleteDate = fakeNowProvider.Today.AddMonths(20);
        goal.Amount = 10000;
        goal.InitialAmount = 0;
        goal.MonthlyContribution = null;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await goalService.ReadGoalsAsync(helper.demoUser.Id, false);

        // Assert
        result.Single().PercentComplete.Should().Be(100);
    }
    #endregion

    #region UpdateGoalAsync
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public static IEnumerable<object[]> UpdateGoalData =>
        [
            [2000m, null],
            [null, "2024-12-31"],
            [null, null],
        ];
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Theory]
    [MemberData(nameof(UpdateGoalData))]
    public async Task UpdateGoalAsync_WhenValidData_ShouldUpdateData(
        decimal? monthlyContribution,
        string? completeDate
    )
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.MonthlyContribution = monthlyContribution;
        goal.CompleteDate = completeDate is not null ? DateOnly.Parse(completeDate) : null;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = new GoalUpdateRequest
        {
            ID = goal.ID,
            Name = "Updated Goal Name",
            Amount = 2000,
            MonthlyContribution = 2000,
            CompleteDate = null,
        };

        // Act
        await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        var updatedGoalEntity = helper.UserDataContext.Goals.Single();
        updatedGoalEntity.Name.Should().Be(updatedGoal.Name);
        updatedGoalEntity.Amount.Should().Be(updatedGoal.Amount);
        updatedGoalEntity.MonthlyContribution.Should().Be(updatedGoal.MonthlyContribution.Value);
        updatedGoalEntity.CompleteDate.Should().Be(updatedGoal.CompleteDate.Value);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenMonthlyContributionIsNull_ShouldAllowUpdate()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.MonthlyContribution = 1000;
        goal.CompleteDate = null;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = new GoalUpdateRequest
        {
            ID = goal.ID,
            Name = "Updated Goal Name",
            Amount = 2000,
            MonthlyContribution = null,
            CompleteDate = null,
        };

        // Act
        await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        var updatedGoalEntity = helper.UserDataContext.Goals.Single();
        updatedGoalEntity.Name.Should().Be(updatedGoal.Name);
        updatedGoalEntity.Amount.Should().Be(updatedGoal.Amount);
        updatedGoalEntity.MonthlyContribution.Should().BeNull();
        updatedGoalEntity.CompleteDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateGoalAsync_InvalidGoalId_ShouldThrowGoalNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updatedGoal = new GoalUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Goal Name",
            Amount = 2000,
            MonthlyContribution = 2000,
            CompleteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
        };

        // Act
        Func<Task> act = async () =>
            await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalNotFoundError");
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenTryToEditBothCompleteDateAndMonthlyContribution_ShouldThrowGoalUpdateBothDateAndContributionError()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.CompleteDate = null;
        goal.MonthlyContribution = 1000;
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = new GoalUpdateRequest
        {
            ID = goal.ID,
            Name = "Updated Goal Name",
            Amount = 2000,
            MonthlyContribution = 2000,
            CompleteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
        };

        // Act
        Func<Task> act = async () =>
            await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalUpdateBothDateAndContributionError");
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenCompleteDateSetToPast_ShouldThrowGoalUpdatePastDateError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = new GoalUpdateRequest
        {
            ID = goal.ID,
            Name = "Updated Goal Name",
            Amount = 2000,
            CompleteDate = fakeNowProvider.Today.AddDays(-1),
        };

        // Act
        Func<Task> act = async () =>
            await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalUpdatePastDateError");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateGoalAsync_WhenMonthlyContributionInvalid_ShouldThrowGoalUpdateNoMonthlyContributionError(
        int? monthlyContribution
    )
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var updatedGoal = new GoalUpdateRequest
        {
            ID = goal.ID,
            Name = "Updated Goal Name",
            Amount = 2000,
            MonthlyContribution = monthlyContribution,
        };

        // Act
        Func<Task> act = async () =>
            await goalService.UpdateGoalAsync(helper.demoUser.Id, updatedGoal);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalUpdateNoMonthlyContributionError");
    }
    #endregion

    #region DeleteGoalAsync
    [Fact]
    public async Task DeleteGoalAsync_WhenValidData_ShouldDeleteGoal()
    {
        // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        // Act
        await goalService.DeleteGoalAsync(helper.demoUser.Id, goal.ID);

        // Assert
        helper.UserDataContext.Goals.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteGoalAsync_WhenInvalidGoal_ShouldThrowGoalNotFoundError()
    { // Arrange
        var helper = new TestHelper();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await goalService.DeleteGoalAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalNotFoundError");
    }
    #endregion

    #region CompleteGoalAsync
    [Fact]
    public async Task CompleteGoalAsync_WhenIncomplete_ShouldMarkComplete()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var completedDate = fakeNowProvider.Today.AddDays(-1);

        // Act
        await goalService.CompleteGoalAsync(helper.demoUser.Id, goal.ID, completedDate);

        // Assert
        helper.UserDataContext.Goals.Should().ContainSingle();
        helper.UserDataContext.Goals.Single().Completed.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteGoalAsync_WhenAlreadyComplete_ShouldThrowGoalCompleteAlreadyCompletedError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);
        var goal = goalFaker.Generate();
        goal.Accounts = accounts;
        goal.Completed = fakeNowProvider.Today;

        helper.UserDataContext.Goals.Add(goal);
        helper.UserDataContext.SaveChanges();

        var completedDate = fakeNowProvider.Today.AddDays(-1);

        // Act
        Func<Task> act = async () =>
            await goalService.CompleteGoalAsync(helper.demoUser.Id, goal.ID, completedDate);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalCompleteAlreadyCompletedError");
    }

    [Fact]
    public async Task CompleteGoalAsync_WhenInvalidGoal_ShouldThrowGoalNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var completedDate = fakeNowProvider.Today.AddDays(-1);

        // Act
        Func<Task> act = async () =>
            await goalService.CompleteGoalAsync(helper.demoUser.Id, Guid.NewGuid(), completedDate);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("GoalNotFoundError");
    }
    #endregion

    #region CompleteEligibleGoalsAsync
    [Fact]
    public async Task CompleteEligibleGoalsAsync_ShouldCompleteEligibleGoals()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        var balanceFaker = new BalanceFaker([.. accounts.Select(a => a.ID)]);
        var newBalance = balanceFaker.Generate();
        newBalance.Amount = 5000;
        newBalance.Date = new DateOnly(fakeNowProvider.Today.Year, fakeNowProvider.Today.Month, 1);
        accounts.Single(a => a.ID == newBalance.AccountID).Balances.Add(newBalance);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);

        var eligibleGoal = goalFaker.Generate();
        eligibleGoal.Accounts = accounts;
        eligibleGoal.Amount = 5000;
        eligibleGoal.InitialAmount = 0;
        eligibleGoal.CompleteDate = null;

        helper.UserDataContext.Goals.Add(eligibleGoal);

        var ineligibleGoal = goalFaker.Generate();
        ineligibleGoal.Accounts = accounts;
        ineligibleGoal.Amount = 6000;
        ineligibleGoal.InitialAmount = 5000;
        ineligibleGoal.CompleteDate = null;

        helper.UserDataContext.Goals.Add(ineligibleGoal);

        helper.UserDataContext.SaveChanges();

        // Act
        await goalService.CompleteEligibleGoalsAsync(helper.demoUser.Id);

        // Assert
        helper.UserDataContext.Goals.Should().HaveCount(2);
        helper
            .UserDataContext.Goals.Single(g => g.ID == eligibleGoal.ID)
            .Completed.Should()
            .NotBeNull();
        helper
            .UserDataContext.Goals.Single(g => g.ID == ineligibleGoal.ID)
            .Completed.Should()
            .BeNull();
    }

    [Fact]
    public async Task CompleteEligibleGoalsAsync_WhenGoalAlreadyCompleted_ShouldNotUpdateCompletedDate()
    {
        // Arrange
        var helper = new TestHelper();
        var fakeNowProvider = CreateNowProviderMock();
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            fakeNowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(5);

        var balanceFaker = new BalanceFaker([.. accounts.Select(a => a.ID)]);
        var newBalance = balanceFaker.Generate();
        newBalance.Amount = 5000;
        newBalance.Date = new DateOnly(fakeNowProvider.Today.Year, fakeNowProvider.Today.Month, 1);
        accounts.Single(a => a.ID == newBalance.AccountID).Balances.Add(newBalance);

        helper.UserDataContext.Accounts.AddRange(accounts);

        var goalFaker = new GoalFaker(helper.demoUser.Id);

        var completedGoal = goalFaker.Generate();
        completedGoal.Accounts = accounts;
        completedGoal.Amount = 5000;
        completedGoal.InitialAmount = 0;
        completedGoal.CompleteDate = null;
        completedGoal.Completed = fakeNowProvider.Today.AddDays(-1);

        helper.UserDataContext.Goals.Add(completedGoal);
        helper.UserDataContext.SaveChanges();

        // Act
        await goalService.CompleteEligibleGoalsAsync(helper.demoUser.Id);

        // Assert
        helper
            .UserDataContext.Goals.Single(g => g.ID == completedGoal.ID)
            .Completed.Should()
            .Be(completedGoal.Completed);
    }
    #endregion

    private static INowProvider CreateNowProviderMock(DateTime? dateTime = null)
    {
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock
            .Setup(np => np.Today)
            .Returns(
                dateTime.HasValue
                    ? DateOnly.FromDateTime(dateTime.Value)
                    : DateOnly.FromDateTime(DateTime.Today)
            );
        return nowProviderMock.Object;
    }
}
