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

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = ".*test.*",
            Category = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value,
        };

        // Act
        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            rule
        );

        // Assert
        helper.demoUser.AutomaticCategorizationRules.Should().HaveCount(1);
        helper
            .demoUser.AutomaticCategorizationRules.First()
            .CategorizationRule.Should()
            .Be(rule.CategorizationRule);
    }

    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenRuleIsInvalid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = "invalid regex[",
            Category = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value,
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
            .WithMessage("Invalid regex in automatic categorization rule.");
    }

    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenRuleAlreadyExists_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = ".*test.*",
            Category = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value,
        };

        await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
            helper.demoUser.Id,
            rule
        );

        // Act
        Func<Task> act = async () =>
            await automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
                helper.demoUser.Id,
                rule
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("An automatic categorization rule with this regex already exists.");
    }

    [Fact]
    public async Task CreateAutomaticCategorizationRuleAsync_WhenCategoryIsInvalid_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();
        var automaticCategorizationRuleService = new AutomaticCategorizationRuleService(
            Mock.Of<ILogger<IAutomaticCategorizationRuleService>>(),
            helper.UserDataContext
        );

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = ".*test.*",
            Category = "InvalidCategory",
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
            .WithMessage("Invalid category provided.");
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

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = ".*test.*",
            Category = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value,
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
        rules.First().CategorizationRule.Should().Be(rule.CategorizationRule);
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

        var rule = new AutomaticCategorizationRuleRequest
        {
            CategorizationRule = ".*test.*",
            Category = TransactionCategoriesConstants.DefaultTransactionCategories.First().Value,
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
