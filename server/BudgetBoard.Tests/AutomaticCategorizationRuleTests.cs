using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AutomaticCategorizationRuleTests
{
    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenRuleIsValid_CreatesRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleCreateRequest
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
        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            rule
        );

        // Assert
        helper.demoUser.AutomaticCategorizationRules.Should().HaveCount(1);
        helper.demoUser.AutomaticCategorizationRules.First().Conditions.Should().HaveCount(1);
        helper.demoUser.AutomaticCategorizationRules.First().Actions.Should().HaveCount(1);
        helper
            .demoUser.AutomaticCategorizationRules.First()
            .Conditions.First()
            .Field.Should()
            .Be("Description");
    }

    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenConditionsAreEmpty_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );
        var rule = new AutomaticCategorizationRuleCreateRequest
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
            await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
                helper.demoUser.Id,
                rule
            );
        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("At least one condition must be provided for the rule.");
    }

    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenActionsAreEmpty_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );
        var rule = new AutomaticCategorizationRuleCreateRequest
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
            await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
                helper.demoUser.Id,
                rule
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("At least one action must be provided for the rule.");
    }

    [Fact]
    public async Task ReadAutomaticCategorizationRulesAsync_WhenCalled_ReturnsRules()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleCreateRequest
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

        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            rule
        );

        // Act
        var rules = await automaticCategorizationRuleService.ReadAutomaticCategorizationRulesAsync(
            helper.demoUser.Id
        );

        // Assert
        rules.Should().HaveCount(1);
        rules.First().Conditions.Should().HaveCount(1);
        rules.First().Actions.Should().HaveCount(1);
        rules.First().Conditions.First().Field.Should().Be("Description");
    }

    [Fact]
    public async Task ReadAutomaticCategorizationRulesAsync_WhenNoRulesExist_ReturnsEmptyList()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        // Act
        var rules = await automaticCategorizationRuleService.ReadAutomaticCategorizationRulesAsync(
            helper.demoUser.Id
        );

        // Assert
        rules.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAutomaticCategorizationRuleAsync_WhenValidData_ShouldUpdateRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var initialRule = new AutomaticCategorizationRuleCreateRequest
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

        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            initialRule
        );

        var createdRuleId = helper.demoUser.AutomaticCategorizationRules.First().ID;
        var createdConditionId = helper
            .demoUser.AutomaticCategorizationRules.First()
            .Conditions.First()
            .ID;
        var createdActionId = helper
            .demoUser.AutomaticCategorizationRules.First()
            .Actions.First()
            .ID;
        var updatedRule = new AutomaticCategorizationRuleUpdateRequest
        {
            ID = createdRuleId,
            Conditions =
            [
                new RuleParameterUpdateRequest
                {
                    ID = createdConditionId,
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
                    ID = createdActionId,
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
        await automaticCategorizationRuleService.UpdateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            updatedRule
        );

        // Assert
        var updatedRuleFromDb = helper.demoUser.AutomaticCategorizationRules.First(r =>
            r.ID == createdRuleId
        );
        updatedRuleFromDb.Conditions.Should().HaveCount(1);
        updatedRuleFromDb.Conditions.First().Field.Should().Be("Amount");
        updatedRuleFromDb.Actions.Should().HaveCount(1);
        updatedRuleFromDb
            .Actions.First()
            .Value.Should()
            .Be(TransactionCategoriesConstants.DefaultTransactionCategories.Last().Value);
    }

    [Fact]
    public async Task UpdateAutomaticCategorizationRuleAsync_WhenRuleDoesNotExist_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var updatedRule = new AutomaticCategorizationRuleUpdateRequest
        {
            ID = Guid.NewGuid(), // Non-existent rule ID
            Conditions =
            [
                new RuleParameterUpdateRequest
                {
                    ID = Guid.NewGuid(),
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
                    ID = Guid.NewGuid(),
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
            await automaticCategorizationRuleService.UpdateAutomaticCategorizationRuleAsync(
                helper.demoUser.Id,
                updatedRule
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Automatic categorization rule not found.");
    }

    [Fact]
    public async Task DeleteAutomaticCategorizationRuleAsync_WhenRuleExists_DeletesRule()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleCreateRequest
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

        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            rule
        );

        // Act
        await automaticCategorizationRuleService.DeleteAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            helper.demoUser.AutomaticCategorizationRules.First().ID
        );

        // Assert
        helper.demoUser.AutomaticCategorizationRules.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAutomaticCategorizationRuleAsync_WhenRuleDoesNotExist_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        // Act
        Func<Task> act = async () =>
            await automaticCategorizationRuleService.DeleteAutomaticCategorizationRuleAsync(
                helper.demoUser.Id,
                Guid.NewGuid() // Non-existent rule ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Automatic categorization rule not found.");
    }
}
