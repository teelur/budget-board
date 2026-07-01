using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.IntegrationTests.Helpers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AutomaticRuleConditionTests
{
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_UnsupportedConditionField_ThrowsAutomaticRuleUnsupportedFieldError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = "unsupportedField",
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "Test",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedFieldError*");
    }

    #region RunOneOffAutomaticRuleAsync_MerchantCondition
    [Theory]
    [InlineData("equals", "acme corp", "ACME Corp", "Other Corp", true, false)]
    [InlineData("notEquals", "ACME Corp", "Other Corp", "ACME Corp", true, false)]
    [InlineData("contains", "acme", "ACME Corporation", "Other Store", true, false)]
    [InlineData("doesNotContain", "acme", "Other Store", "ACME Corporation", true, false)]
    [InlineData("startsWith", "acme", "ACME Store", "Best ACME", true, false)]
    [InlineData("endsWith", "acme", "Store ACME", "ACME Store", true, false)]
    [InlineData("matchesRegex", @"^ACME\d+$", "ACME123", "Other Store", true, false)]
    public async Task RunOneOffAutomaticRuleAsync_MerchantCondition_AppliesExpectedFiltering(
        string operatorName,
        string value,
        string firstMerchantName,
        string secondMerchantName,
        bool expectFirstUpdate,
        bool expectSecondUpdate
    )
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var first = new TransactionFaker([account.ID]).Generate();
        first.MerchantName = firstMerchantName;
        var second = new TransactionFaker([account.ID]).Generate();
        second.MerchantName = secondMerchantName;
        var third = new TransactionFaker([account.ID]).Generate();
        third.MerchantName = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(first, second, third);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = operatorName,
                    Value = value,
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == first.ID)
                ),
            expectFirstUpdate ? Times.Once() : Times.Never()
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == second.ID)
                ),
            expectSecondUpdate ? Times.Once() : Times.Never()
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_MerchantCondition_InvalidRegex_ThrowsAutomaticRuleInvalidRegexError()
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
                    Operator = AutomaticRuleConstants.ConditionalOperators.MatchesRegex,
                    Value = "[invalid regex",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidRegexError*");
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_MerchantCondition_UnsupportedOperator_ThrowsAutomaticRuleUnsupportedOperatorForMerchantError()
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
                    Operator = "unsupportedOperator",
                    Value = "some value",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForMerchantError*");
    }
    #endregion

    #region RunOneOffAutomaticRuleAsync_CategoryCondition
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_CategoryDoesNotExist_ThrowsAutomaticRuleCategoryDoesNotExistError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = AutomaticRuleConstants.ConditionalOperators.Is,
                    Value = "NonexistentCategory",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleCategoryDoesNotExistError*");
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_Is_ParentCategory_FiltersOnCategoryField()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Category = "Auto & Transport";
        matching.Subcategory = string.Empty;
        var other = new TransactionFaker([account.ID]).Generate();
        other.Category = "Bills & Utilities";
        other.Subcategory = string.Empty;
        var nullCase = new TransactionFaker([account.ID]).Generate();
        nullCase.Category = null;
        nullCase.Subcategory = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other, nullCase);
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
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_Is_Subcategory_FiltersOnSubcategoryField()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var matching = new TransactionFaker([account.ID]).Generate();
        matching.Category = "Auto & Transport";
        matching.Subcategory = "Auto Insurance";
        var other = new TransactionFaker([account.ID]).Generate();
        other.Category = "Auto & Transport";
        other.Subcategory = "Gas & Fuel";
        var nullCase = new TransactionFaker([account.ID]).Generate();
        nullCase.Category = null;
        nullCase.Subcategory = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(matching, other, nullCase);
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
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_IsNot_ParentCategory_ExcludesMatchingCategory()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.Category = "Auto & Transport";
        excluded.Subcategory = string.Empty;
        var included = new TransactionFaker([account.ID]).Generate();
        included.Category = "Bills & Utilities";
        included.Subcategory = string.Empty;
        var nullCase = new TransactionFaker([account.ID]).Generate();
        nullCase.Category = null;
        nullCase.Subcategory = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included, nullCase);
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
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_IsNot_Subcategory_ExcludesMatchingSubcategory()
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var excluded = new TransactionFaker([account.ID]).Generate();
        excluded.Category = "Auto & Transport";
        excluded.Subcategory = "Auto Insurance";
        var included = new TransactionFaker([account.ID]).Generate();
        included.Category = "Auto & Transport";
        included.Subcategory = "Gas & Fuel";
        var nullCase = new TransactionFaker([account.ID]).Generate();
        nullCase.Category = null;
        nullCase.Subcategory = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(excluded, included, nullCase);
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
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_CategoryCondition_UnsupportedOperator_ThrowsAutomaticRuleUnsupportedOperatorForCategoryError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Category,
                    Operator = "contains",
                    Value = "Auto & Transport",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForCategoryError*");
    }
    #endregion

    #region RunOneOffAutomaticRuleAsync_AmountCondition
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountCondition_InvalidAmount_ThrowsException()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = AutomaticRuleConstants.ConditionalOperators.EqualsString,
                    Value = "not-a-number",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAmountError*");
    }

    [Theory]
    [InlineData("equals", "100.00", 100.00, 200.00, true, false)]
    [InlineData("notEquals", "100.00", 200.00, 100.00, true, false)]
    [InlineData("greaterThan", "100.00", 150.00, 50.00, true, false)]
    [InlineData("lessThan", "100.00", 50.00, 150.00, true, false)]
    public async Task RunOneOffAutomaticRuleAsync_AmountCondition_FiltersCorrectly(
        string operatorName,
        string conditionValue,
        decimal firstAmount,
        decimal secondAmount,
        bool expectFirstUpdate,
        bool expectSecondUpdate
    )
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var first = new TransactionFaker([account.ID]).Generate();
        first.Amount = firstAmount;
        var second = new TransactionFaker([account.ID]).Generate();
        second.Amount = secondAmount;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(first, second);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = operatorName,
                    Value = conditionValue,
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == first.ID)
                ),
            expectFirstUpdate ? Times.Once() : Times.Never()
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == second.ID)
                ),
            expectSecondUpdate ? Times.Once() : Times.Never()
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AmountCondition_UnsupportedOperator_ThrowsAutomaticRuleUnsupportedOperatorForAmountError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Amount,
                    Operator = "contains",
                    Value = "100",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForAmountError*");
    }
    #endregion

    #region RunOneOffAutomaticRuleAsync_DateCondition
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_DateCondition_InvalidDate_ThrowsAutomaticRuleInvalidDateError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = AutomaticRuleConstants.ConditionalOperators.On,
                    Value = "not-a-date",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidDateError*");
    }

    [Theory]
    [InlineData("on", "2024-01-15", "2024-01-15", "2024-01-16", true, false)]
    [InlineData("before", "2024-01-15", "2024-01-10", "2024-01-20", true, false)]
    [InlineData("after", "2024-01-15", "2024-01-20", "2024-01-10", true, false)]
    public async Task RunOneOffAutomaticRuleAsync_DateCondition_FiltersCorrectly(
        string operatorName,
        string conditionValue,
        string firstDateValue,
        string secondDateValue,
        bool expectFirstUpdate,
        bool expectSecondUpdate
    )
    {
        var helper = new TestHelper();
        var mock = AutomaticRuleTestHelpers.UpdateMock();
        var service = AutomaticRuleTestHelpers.BuildService(helper, mock.Object);

        var account = new AccountFaker(helper.demoUser.Id).Generate();
        var first = new TransactionFaker([account.ID]).Generate();
        first.Date = DateOnly.Parse(firstDateValue);
        var second = new TransactionFaker([account.ID]).Generate();
        second.Date = DateOnly.Parse(secondDateValue);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(first, second);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = operatorName,
                    Value = conditionValue,
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == first.ID)
                ),
            expectFirstUpdate ? Times.Once() : Times.Never()
        );
        mock.Verify(
            ts =>
                ts.UpdateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.Is<ITransactionUpdateRequest>(r => r.ID == second.ID)
                ),
            expectSecondUpdate ? Times.Once() : Times.Never()
        );
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_DateCondition_UnsupportedOperator_ThrowsAutomaticRuleUnsupportedOperatorForDateError()
    {
        var helper = new TestHelper();
        var service = AutomaticRuleTestHelpers.BuildService(helper);

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Date,
                    Operator = "contains",
                    Value = "2024-01-15",
                },
            ],
            Actions = [AutomaticRuleTestHelpers.MerchantSetAction()],
        };

        Func<Task> act = async () =>
            await service.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForDateError*");
    }
    #endregion

    #region RunOneOffAutomaticRuleAsync_AccountCondition
    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AccountCondition_CommaSeparatedWithInvalidGuid_ThrowsException()
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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                },
            ],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleInvalidAccountIdError*");
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AccountCondition_Is_CommaSeparatedGuids_IncludesAllListedAccounts()
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
        var transactionA = new TransactionFaker([accountA.ID]).Generate();
        var transactionB = new TransactionFaker([accountB.ID]).Generate();

        helper.UserDataContext.Accounts.AddRange(accountA, accountB);
        helper.UserDataContext.Transactions.AddRange(transactionA, transactionB);
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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                },
            ],
        };

        // Act
        await automaticRuleService.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    }

    [Fact]
    public async Task RunOneOffAutomaticRuleAsync_AccountCondition_IsNot_CommaSeparatedGuids_ExcludesAllListedAccounts()
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
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                },
            ],
        };

        // Act
        await automaticRuleService.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

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
    public async Task RunOneOffAutomaticRuleAsync_AccountCondition_UnsupportedOperator_ThrowsAutomaticRuleUnsupportedOperatorForAccountError()
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

        var accountA = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(accountA);
        helper.UserDataContext.SaveChanges();

        var rule = new AutomaticRuleCreateRequest
        {
            Conditions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Account,
                    Operator = "contains",
                    Value = accountA.ID.ToString(),
                },
            ],
            Actions =
            [
                new RuleParameterCreateRequest
                {
                    Field = AutomaticRuleConstants.TransactionFields.Merchant,
                    Operator = AutomaticRuleConstants.ActionOperators.Set,
                    Value = "updated",
                },
            ],
        };

        // Act
        Func<Task> act = async () =>
            await automaticRuleService.RunOneOffAutomaticRuleAsync(helper.demoUser.Id, rule);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutomaticRuleUnsupportedOperatorForAccountError*");
    }
    #endregion
}
