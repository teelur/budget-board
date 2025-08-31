using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
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
            helper.UserDataContext
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
            helper.UserDataContext
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
            .WithMessage("At least one condition must be provided for the rule.");
    }

    [Fact]
    public async Task CreateAutomaticRuleAsync_WhenActionsAreEmpty_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
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
            .WithMessage("At least one action must be provided for the rule.");
    }

    [Fact]
    public async Task ReadAutomaticRulesAsync_WhenCalled_ReturnsRules()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
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

        await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Act
        var rules = await automaticRuleService.ReadAutomaticRulesAsync(helper.demoUser.Id);

        // Assert
        rules.Should().HaveCount(1);
        rules.First().Conditions.Should().HaveCount(1);
        rules.First().Actions.Should().HaveCount(1);
        rules.First().Conditions.First().Field.Should().Be("Description");
    }

    [Fact]
    public async Task ReadAutomaticRulesAsync_WhenNoRulesExist_ReturnsEmptyList()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
        );

        // Act
        var rules = await automaticRuleService.ReadAutomaticRulesAsync(helper.demoUser.Id);

        // Assert
        rules.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAutomaticRuleAsync_WhenValidData_ShouldUpdateRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
        );

        var initialRule = new AutomaticRuleCreateRequest
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

        await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, initialRule);

        var createdRuleId = helper.demoUser.AutomaticRules.First().ID;
        var createdConditionId = helper.demoUser.AutomaticRules.First().Conditions.First().ID;
        var createdActionId = helper.demoUser.AutomaticRules.First().Actions.First().ID;
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
            helper.UserDataContext
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
            .WithMessage("Automatic  rule not found.");
    }

    [Fact]
    public async Task DeleteAutomaticRuleAsync_WhenRuleExists_DeletesRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
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

        await automaticRuleService.CreateAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Act
        await automaticRuleService.DeleteAutomaticRuleAsync(
            helper.demoUser.Id,
            helper.demoUser.AutomaticRules.First().ID
        );

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
            helper.UserDataContext
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
            .WithMessage("Automatic  rule not found.");
    }
}
