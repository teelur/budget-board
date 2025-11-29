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
}
