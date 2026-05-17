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
public class AutomaticRuleTests
{
    [Fact]
    public async Task CreateAutomaticRuleAsync_WhenRuleIsValid_CreatesRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "Description",
                    Operator = "matches",
                    Value = ".*test.*",
                    Type = "string",
                },
            ],

            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "Category",
                    Operator = "set",
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.First()
                        .Value,
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        helper.demoUser.AutomaticRules.Should().HaveCount(1);
        helper.demoUser.AutomaticRules.First().Conditions.Should().HaveCount(1);
        helper.demoUser.AutomaticRules.First().Actions.Should().HaveCount(1);
        helper.demoUser.AutomaticRules.First().Conditions.First().Field.Should().Be("Description");
    }

    [Fact]
    public async Task CreateAutomaticRuleAsync_WhenConditionsAreEmpty_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var rule = new AutomaticRuleCreateRequest
        {
            Conditions = [],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "Category",
                    Operator = "set",
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.First()
                        .Value,
                    Type = "string",
                },
            ],
        };
        // Act
        Func<Task> act = async () =>
            await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NoConditionsCreateError");
    }

    [Fact]
    public async Task CreateAutomaticRuleAsync_WhenActionsAreEmpty_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "Description",
                    Operator = "matches",
                    Value = ".*test.*",
                    Type = "string",
                },
            ],
            Actions = [],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NoActionsCreateError");
    }

    [Fact]
    public async Task ReadAutomaticRulesAsync_WhenCalled_ReturnsRules()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var automaticRuleFaker = new AutomaticRuleFaker(helper.demoUser.Id);
        var demoRules = automaticRuleFaker.Generate(5);

        helper.UserDataContext.AutomaticRules.AddRange(demoRules);
        helper.UserDataContext.SaveChanges();

        // Act
        var rules = await automaticRuleService.ReadAutomaticRulesAsync(helper.demoUser.Id);

        // Assert
        rules.Should().HaveCount(5);
        foreach (var rule in demoRules)
        {
            rules.Should().ContainSingle(r => r.ID == rule.ID);
            rules.First(r => r.ID == rule.ID).Conditions.Should().HaveCount(rule.Conditions.Count);
            rules.First(r => r.ID == rule.ID).Actions.Should().HaveCount(rule.Actions.Count);
        }
    }

    [Fact]
    public async Task UpdateAutomaticRuleAsync_WhenValidData_ShouldUpdateRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var automaticRuleFaker = new AutomaticRuleFaker(helper.demoUser.Id);
        var demoRule = automaticRuleFaker.Generate();

        helper.UserDataContext.AutomaticRules.Add(demoRule);
        helper.UserDataContext.SaveChanges();

        var createdRuleId = helper.demoUser.AutomaticRules.First().ID;
        var updatedRule = new AutomaticRuleUpdateRequest
        {
            ID = createdRuleId,
            Conditions =
            [
                new RuleParameterUpdateRequest
                {
                    Field = "Amount",
                    Operator = "greater_than",
                    Value = "100",
                    Type = "number",
                },
            ],
            Actions =
            [
                new RuleParameterUpdateRequest
                {
                    Field = "Category",
                    Operator = "set",
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.Last()
                        .Value,
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.UpdateAutomaticRuleAsync(helper.demoUser.Id, updatedRule);

        // Assert
        var updatedRuleFromDb = helper.demoUser.AutomaticRules.First(r => r.ID == createdRuleId);
        updatedRuleFromDb.Conditions.Should().HaveCount(1);
        updatedRuleFromDb.Conditions.First().Field.Should().Be("Amount");
        updatedRuleFromDb.Actions.Should().HaveCount(1);
        updatedRuleFromDb
            .Actions.First()
            .Value.Should()
            .Be(TransactionCategoriesConstants.DefaultTransactionCategories.Last().Value);
    }

    [Fact]
    public async Task UpdateAutomaticRuleAsync_WhenRuleDoesNotExist_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updatedRule = new AutomaticRuleUpdateRequest
        {
            ID = Guid.NewGuid(), // Non-existent rule ID
            Conditions =
            [
                new RuleParameterUpdateRequest
                {
                    Field = "Amount",
                    Operator = "greater_than",
                    Value = "100",
                    Type = "number",
                },
            ],
            Actions =
            [
                new RuleParameterUpdateRequest
                {
                    Field = "Category",
                    Operator = "set",
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.Last()
                        .Value,
                    Type = "string",
                },
            ],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.UpdateAutomaticRuleAsync(helper.demoUser.Id, updatedRule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteAutomaticRuleAsync_WhenRuleExists_DeletesRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var automaticRuleFaker = new AutomaticRuleFaker(helper.demoUser.Id);
        var demoRule = automaticRuleFaker.Generate();

        helper.UserDataContext.AutomaticRules.Add(demoRule);
        helper.UserDataContext.SaveChanges();

        // Act
        await automaticRuleService.DeleteAutomaticRuleAsync(helper.demoUser.Id, demoRule.ID);

        // Assert
        helper.demoUser.AutomaticRules.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAutomaticRuleAsync_WhenRuleDoesNotExist_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.DeleteAutomaticRuleAsync(
                helper.demoUser.Id,
                Guid.NewGuid() // Non-existent rule ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleDeleteNotFoundError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_WhenValidRule_ShouldRunRule()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.MatchesRegex,
                    Value = ".*test.*",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.First()
                        .Value,
                    Type = "string",
                },
            ],
        };

        // Act
        var result = await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        result.Should().Contain("RuleRunSummary");
    }

    [Fact]
    public async Task RunAutomaticRulesAsync_WhenCalled_ShouldRunAllRules()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "This is a test merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var automaticRule = new AutomaticRule()
        {
            ID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            Conditions =
            [
                new RuleCondition
                {
                    ID = Guid.NewGuid(),
                    RuleID = Guid.NewGuid(),
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Contains,
                    Value = "test",
                },
            ],
            Actions =
            [
                new RuleAction
                {
                    ID = Guid.NewGuid(),
                    RuleID = Guid.NewGuid(),
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = TransactionCategoriesConstants
                        .DefaultTransactionCategories.First()
                        .Value,
                },
            ],
        };

        helper.UserDataContext.AutomaticRules.Add(automaticRule);
        helper.UserDataContext.SaveChanges();

        // Act
        await automaticRuleService.RunAutomaticRulesAsync(helper.demoUser.Id);

        // Assert
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_Is_SingleGuid_FiltersMatchingAccount()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var matchingAccount = new AccountFaker(helper.demoUser.Id).Generate();
        var otherAccount = new AccountFaker(helper.demoUser.Id).Generate();
        var matchingTransaction = new TransactionFaker([matchingAccount.ID]).Generate();
        var otherTransaction = new TransactionFaker([otherAccount.ID]).Generate();

        helper.UserDataContext.Accounts.AddRange(matchingAccount, otherAccount);
        helper.UserDataContext.Transactions.AddRange(matchingTransaction, otherTransaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = matchingAccount.ID.ToString(),
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matchingTransaction.ID)
                ),
            Times.Once
        );
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == otherTransaction.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_IsNot_SingleGuid_ExcludesMatchingAccount()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var excludedAccount = new AccountFaker(helper.demoUser.Id).Generate();
        var otherAccount = new AccountFaker(helper.demoUser.Id).Generate();
        var excludedTransaction = new TransactionFaker([excludedAccount.ID]).Generate();
        var otherTransaction = new TransactionFaker([otherAccount.ID]).Generate();

        helper.UserDataContext.Accounts.AddRange(excludedAccount, otherAccount);
        helper.UserDataContext.Transactions.AddRange(excludedTransaction, otherTransaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.IsNot,
                    Value = excludedAccount.ID.ToString(),
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == otherTransaction.ID)
                ),
            Times.Once
        );
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excludedTransaction.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_Is_CommaSeparatedGuids_FiltersAllMatchingAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountA = new AccountFaker(helper.demoUser.Id).Generate();
        var accountB = new AccountFaker(helper.demoUser.Id).Generate();
        var accountC = new AccountFaker(helper.demoUser.Id).Generate();
        var transactionA = new TransactionFaker([accountA.ID]).Generate();
        var transactionB = new TransactionFaker([accountB.ID]).Generate();
        var transactionC = new TransactionFaker([accountC.ID]).Generate();

        helper.UserDataContext.Accounts.AddRange(accountA, accountB, accountC);
        helper.UserDataContext.Transactions.AddRange(transactionA, transactionB, transactionC);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = $"{accountA.ID},{accountB.ID}",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == transactionA.ID)
                ),
            Times.Once
        );
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == transactionB.ID)
                ),
            Times.Once
        );
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == transactionC.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_IsNot_CommaSeparatedGuids_ExcludesAllListedAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock
            .Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountA = new AccountFaker(helper.demoUser.Id).Generate();
        var accountB = new AccountFaker(helper.demoUser.Id).Generate();
        var accountC = new AccountFaker(helper.demoUser.Id).Generate();
        var transactionA = new TransactionFaker([accountA.ID]).Generate();
        var transactionB = new TransactionFaker([accountB.ID]).Generate();
        var transactionC = new TransactionFaker([accountC.ID]).Generate();

        helper.UserDataContext.Accounts.AddRange(accountA, accountB, accountC);
        helper.UserDataContext.Transactions.AddRange(transactionA, transactionB, transactionC);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.IsNot,
                    Value = $"{accountA.ID},{accountB.ID}",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == transactionC.ID)
                ),
            Times.Once
        );
        transactionServiceMock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transactionA.ID || r.ID == transactionB.ID
                    )
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_InvalidGuid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = "not-a-valid-guid",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAccountIdError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AccountCondition_CommaSeparatedWithInvalidGuid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();

        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validGuid = Guid.NewGuid();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = $"{validGuid},not-a-valid-guid",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                    Type = "string",
                },
            ],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAccountIdError");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AutomaticRuleService BuildService(
        TestHelper helper,
        ITransactionService? svc = null
    ) =>
        new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            svc ?? Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

    private static Mock<ITransactionService> UpdateMock()
    {
        var mock = new Mock<ITransactionService>();
        mock.Setup(ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>())
            )
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<ITransactionService> DeleteMock()
    {
        var mock = new Mock<ITransactionService>();
        mock.Setup(ts => ts.DeleteTransactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static RuleParameterCreateRequest MerchantSetAction(string value = "updated") =>
        new RuleParameterCreateRequest
        {
            Field = AutomaticRuleConstants.TransactionFields.Merchant,
            Operator = AutomaticRuleConstants.ActionOperators.Set,
            Value = value,
            Type = "string",
        };

    // -----------------------------------------------------------------------
    // Merchant conditions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_EqualsString_FiltersExactMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "ACME Corp";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Other Corp";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "acme corp",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_NotEquals_ExcludesExactMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.MerchantName = "ACME Corp";
        var included = new TransactionFaker([account.ID]).Generate();
        included.MerchantName = "Other Corp";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.NotEquals,
                    Value = "ACME Corp",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == included.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excluded.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_Contains_FiltersSubstringMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "ACME Corporation";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Other Store";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Contains,
                    Value = "acme",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_NotContains_ExcludesSubstringMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.MerchantName = "ACME Corporation";
        var included = new TransactionFaker([account.ID]).Generate();
        included.MerchantName = "Other Store";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.NotContains,
                    Value = "acme",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == included.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excluded.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_StartsWith_FiltersPrefixMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "ACME Store";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Best ACME";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.StartsWith,
                    Value = "acme",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_EndsWith_FiltersSuffixMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "Store ACME";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "ACME Store";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EndsWith,
                    Value = "acme",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_MatchesRegex_FiltersRegexMatch()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "ACME123";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Other Store";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.MatchesRegex,
                    Value = @"^ACME\d+$",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_InvalidRegex_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.MatchesRegex,
                    Value = "[invalid regex",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidRegexError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantCondition_UnsupportedOperator_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = "unsupportedOperator",
                    Value = "some value",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForMerchantError");
    }

    // -----------------------------------------------------------------------
    // Category conditions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_Is_ParentCategory_FiltersOnCategoryField()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Category = "Auto & Transport";
        matching.Subcategory = string.Empty;
        var other = new TransactionFaker([account.ID]).Generate();
        other.Category = "Bills & Utilities";
        other.Subcategory = string.Empty;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = "Auto & Transport",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_Is_Subcategory_FiltersOnSubcategoryField()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Category = "Auto & Transport";
        matching.Subcategory = "Auto Insurance";
        var other = new TransactionFaker([account.ID]).Generate();
        other.Category = "Auto & Transport";
        other.Subcategory = "Gas & Fuel";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = "Auto Insurance",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_IsNot_ParentCategory_ExcludesMatchingCategory()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.Category = "Auto & Transport";
        excluded.Subcategory = string.Empty;
        var included = new TransactionFaker([account.ID]).Generate();
        included.Category = "Bills & Utilities";
        included.Subcategory = string.Empty;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.IsNot,
                    Value = "Auto & Transport",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == included.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excluded.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_IsNot_Subcategory_ExcludesMatchingSubcategory()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.Category = "Auto & Transport";
        excluded.Subcategory = "Auto Insurance";
        var included = new TransactionFaker([account.ID]).Generate();
        included.Category = "Auto & Transport";
        included.Subcategory = "Gas & Fuel";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.IsNot,
                    Value = "Auto Insurance",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == included.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excluded.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_CategoryDoesNotExist_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = "NonexistentCategory",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleCategoryDoesNotExistError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategoryCondition_UnsupportedOperator_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = "contains",
                    Value = "Auto & Transport",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForCategoryError");
    }

    // -----------------------------------------------------------------------
    // Amount conditions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_EqualsString_FiltersExactAmount()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Amount = 100.00m;
        var other = new TransactionFaker([account.ID]).Generate();
        other.Amount = 200.00m;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "100.00",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_NotEquals_ExcludesExactAmount()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.Amount = 100.00m;
        var included = new TransactionFaker([account.ID]).Generate();
        included.Amount = 200.00m;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.NotEquals,
                    Value = "100.00",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == included.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == excluded.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_GreaterThan_FiltersHigherAmounts()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Amount = 150.00m;
        var other = new TransactionFaker([account.ID]).Generate();
        other.Amount = 50.00m;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.GreaterThan,
                    Value = "100.00",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_LessThan_FiltersLowerAmounts()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Amount = 50.00m;
        var other = new TransactionFaker([account.ID]).Generate();
        other.Amount = 150.00m;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.LessThan,
                    Value = "100.00",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_InvalidAmount_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "not-a-number",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAmountError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountCondition_UnsupportedOperator_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = "contains",
                    Value = "100",
                    Type = "number",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForAmountError");
    }

    // -----------------------------------------------------------------------
    // Date conditions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAutomaticRuleAsync_DateCondition_On_FiltersExactDate()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Date = new DateOnly(2024, 1, 15);
        var other = new TransactionFaker([account.ID]).Generate();
        other.Date = new DateOnly(2024, 1, 16);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.On,
                    Value = "2024-01-15",
                    Type = "date",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateCondition_Before_FiltersEarlierDates()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Date = new DateOnly(2024, 1, 10);
        var other = new TransactionFaker([account.ID]).Generate();
        other.Date = new DateOnly(2024, 1, 20);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Before,
                    Value = "2024-01-15",
                    Type = "date",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateCondition_After_FiltersLaterDates()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Date = new DateOnly(2024, 1, 20);
        var other = new TransactionFaker([account.ID]).Generate();
        other.Date = new DateOnly(2024, 1, 10);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.After,
                    Value = "2024-01-15",
                    Type = "date",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == matching.ID)
                ),
            Times.Once
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == other.ID)
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateCondition_InvalidDate_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.On,
                    Value = "not-a-date",
                    Type = "date",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidDateError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateCondition_UnsupportedOperator_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = "contains",
                    Value = "2024-01-15",
                    Type = "date",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForDateError");
    }

    // -----------------------------------------------------------------------
    // Actions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAutomaticRuleAsync_DeleteAction_DeletesMatchingTransactions()
    {
        var helper = new TestHelper();
        var mock = DeleteMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "Delete Me";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Keep Me";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Delete Me",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Delete,
                    Value = string.Empty,
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(ts => ts.DeleteTransactionAsync(It.IsAny<Guid>(), matching.ID), Times.Once);
        mock.Verify(ts => ts.DeleteTransactionAsync(It.IsAny<Guid>(), other.ID), Times.Never);
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_MerchantSetAction_SetsCorrectMerchantName()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Old Name";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Old Name",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "New Name",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transaction.ID && r.MerchantName == "New Name"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategorySetAction_ParentCategory_SetsCorrectCategory()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "Auto & Transport",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transaction.ID
                        && r.Category == "Auto & Transport"
                        && r.Subcategory == string.Empty
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategorySetAction_Subcategory_SetsParentAndSubcategory()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "Auto Insurance",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transaction.ID
                        && r.Category == "Auto & Transport"
                        && r.Subcategory == "Auto Insurance"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountSetAction_SetsCorrectAmount()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Amount = 100.00m;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "100.00",
                    Type = "number",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "150.50",
                    Type = "number",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transaction.ID && r.Amount == 150.50m
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateSetAction_SetsCorrectDate()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Date = new DateOnly(2024, 1, 15);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.On,
                    Value = "2024-01-15",
                    Type = "date",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "2024-06-15",
                    Type = "date",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r =>
                        r.ID == transaction.ID && r.Date == new DateOnly(2024, 6, 15)
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_CategorySetAction_CategoryNotFound_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "NonexistentCategory",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_AmountSetAction_InvalidAmount_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "not-a-number",
                    Type = "number",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_DateSetAction_InvalidDate_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "not-a-date",
                    Type = "date",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_UnsupportedActionOperator_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = "unsupportedOperator",
                    Value = "value",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_UnsupportedConditionField_ThrowsException()
    {
        var helper = new TestHelper();
        var service = BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "unsupportedField",
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test",
                    Type = "string",
                },
            ],
            Actions = [MerchantSetAction()],
        };

        Func<Task> act = async () => await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedFieldError");
    }

    [Fact]
    public async Task RunAutomaticRuleAsync_UnsupportedActionField_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = UpdateMock();
        var service = BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test Merchant",
                    Type = "string",
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "value",
                    Type = "string",
                },
            ],
        };

        await service.RunAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }
}
