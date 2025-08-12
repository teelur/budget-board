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
}
