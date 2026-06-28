using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.IntegrationTests.Helpers;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AutomaticRuleActionTests
{
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_InvalidOperator_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = "unsupportedOperator",
                    Value = "value",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    #region RunOneOffAutomaticRuleAsync_DeleteAction
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_DeleteAction_DeletesMatchingTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.DeleteMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.MerchantName = "Delete Me";
        var otherMatching = new TransactionFaker([account.ID]).Generate();
        otherMatching.MerchantName = "Delete Me";
        var other = new TransactionFaker([account.ID]).Generate();
        other.MerchantName = "Keep Me";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, otherMatching, other);
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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Delete,
                    Value = string.Empty,
                },
            ],
        };

        // Act
        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        mock.Verify(
            ts =>
                ts.DeleteTransactionBatchAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Contains(matching.ID)
                        && ids.Contains(otherMatching.ID)
                        && !ids.Contains(other.ID)
                    )
                ),
            Times.Once
        );
    }
    #endregion

    #region RunOneOffAutomaticRuleAsync_SetOperator
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Merchant_SetsCorrectMerchantName()
    {
        // Arrange
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "New Name",
                },
            ],
        };

        // Act
        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
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
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Category_ParentCategory_SetsCorrectCategory()
    {
        // Arrange
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "Auto & Transport",
                },
            ],
        };

        // Act
        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
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
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Category_Subcategory_SetsParentAndSubcategory()
    {
        // Arrange
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "Auto Insurance",
                },
            ],
        };

        // Act
        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
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
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Category_CategoryNotFound_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "NonexistentCategory",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Amount_SetsCorrectAmount()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "150.50",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Amount_InvalidAmount_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "not-a-number",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Date_SetsCorrectDate()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "2024-06-15",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Date_InvalidDate_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "not-a-date",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_UnsupportedField_DoesNotUpdateTransaction()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "UnsupportedField",
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "value",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionUpdateRequest>()),
            Times.Never
        );
    }
    #endregion
}
