using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.IntegrationTests.Helpers;
using BudgetBoard.Service.Interfaces;
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                ),
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
                ts.DeleteTransactionsAsync(
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID && r.First().MerchantName.Value == "New Name"
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                        && r.First().Category.Value == "Auto & Transport"
                        && r.First().Subcategory.Value == string.Empty
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                        && r.First().Category.Value == "Auto & Transport"
                        && r.First().Subcategory.Value == "Auto Insurance"
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                    )
                ),
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID && r.First().Amount == 150.50m
                    )
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("amount + 50", 150.0)]
    [InlineData("amount - 25", 75.0)]
    [InlineData("amount * 1.5", 150.0)]
    [InlineData("amount / 2", 50.0)]
    [InlineData("amount - 150", -50.0)]
    [InlineData("2 * (amount + 34) / 5", 53.6)]
    [InlineData(" amount + amount / 2 ", 150.0)]
    [InlineData("-amount + 250", 150.0)]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_AppliesExpression(
        string expression,
        double expectedAmount
    )
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Amount = 100.00m;
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
                    Value = expression,
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(requests =>
                        requests.First().ID == transaction.ID
                        && requests.First().Amount == (decimal)expectedAmount
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpressions_AreAppliedInOrder()
    {
        var helper = new TestHelper();
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Amount = 100.00m;
        transaction.MerchantName = "Test Merchant";
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        var updatedAmounts = new List<decimal>();
        var mock = new Mock<ITransactionService>();
        mock.Setup(ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                )
            )
            .Callback<Guid, IEnumerable<ITransactionUpdateRequest>, bool>(
                (_, requests, _) =>
                {
                    var amount = requests.First().Amount!.Value;
                    updatedAmounts.Add(amount);
                    transaction.Amount = amount;
                }
            )
            .Returns(Task.CompletedTask);
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);
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
                    Value = "amount + 50",
                },
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "amount * 2",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        updatedAmounts.Should().Equal(150m, 300m);
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_DivideByZero_IsRejected()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);
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
                    Value = "amount / 0",
                },
            ],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleDivisionByZeroError");
        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_DynamicDivideByZero_DoesNotUpdate()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Amount = 100m;
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
                    Value = "amount / (amount - amount)",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_Overflow_IsRejected()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);
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
                    Value = "79228162514264337593543950336",
                },
            ],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleArithmeticOverflowError");
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_RuntimeOverflow_DoesNotUpdate()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.Amount = decimal.MaxValue;
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
                    Value = "amount * 2",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                ),
            Times.Never
        );
    }

    [Theory]
    [InlineData("amount +")]
    [InlineData("amount + (2")]
    [InlineData("balance + 1")]
    public async Task RunOneOffAutomaticRuleAsync_AmountExpression_InvalidExpression_IsRejected(
        string expression
    )
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);
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
                    Value = expression,
                },
            ],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAmountExpressionError*");
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Amount_InvalidExpression_IsRejected()
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

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAmountExpressionError*");

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                    )
                ),
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                        && r.First().Date == new DateOnly(2024, 6, 15)
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                    )
                ),
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
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(r =>
                        r.First().ID == transaction.ID
                    )
                ),
            Times.Never
        );
    }

    [Theory]
    [InlineData("new note")]
    [InlineData("")]
    public async Task RunOneOffAutomaticRuleAsync_SetOperator_Note_ReplacesExistingNote(
        string newNote
    )
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var transaction = new TransactionFaker([account.ID]).Generate();
        transaction.MerchantName = "Test Merchant";
        transaction.Notes = "Old note";

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
                    Field = AutomaticRuleConstants.TransactionFields.Note,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = newNote,
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(requests =>
                        requests.First().ID == transaction.ID && requests.First().Notes == newNote
                    )
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("add")]
    [InlineData("remove")]
    public async Task RunOneOffAutomaticRuleAsync_TagOperator_PassesJsonTagValues(
        string actionOperator
    )
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
                    Field = AutomaticRuleConstants.TransactionFields.Tags,
                    Operator = actionOperator,
                    Value = "[\"work\", \" important \"]",
                },
            ],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<ITransactionUpdateRequest>>(requests =>
                        requests.First().ID == transaction.ID
                        && (
                            actionOperator == "add"
                                ? requests.First().AddTags != null
                                    && requests
                                        .First()
                                        .AddTags!.SequenceEqual(new[] { "work", " important " })
                                : requests.First().RemoveTags != null
                                    && requests
                                        .First()
                                        .RemoveTags!.SequenceEqual(new[] { "work", " important " })
                        )
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_TagOperator_InvalidJson_IsRejected()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);
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
                    Field = AutomaticRuleConstants.TransactionFields.Tags,
                    Operator = AutomaticRuleConstants.ActionOperators.Add,
                    Value = "work,important",
                },
            ],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidTagsError");
        mock.Verify(
            ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                ),
            Times.Never
        );
    }
    #endregion
}
