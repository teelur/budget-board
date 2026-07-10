using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests.Helpers;

internal class AutomaticRuleTestHelpers
{
    internal static AutomaticRuleService BuildService(
        TestHelper helper,
        ITransactionService? transactionService = null
    ) =>
        new(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext,
            transactionService ?? Mock.Of<ITransactionService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

    internal static Mock<ITransactionService> UpdateMock()
    {
        var mock = new Mock<ITransactionService>();
        mock.Setup(ts =>
                ts.UpdateTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<ITransactionUpdateRequest>>()
                )
            )
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static Mock<ITransactionService> DeleteMock()
    {
        var mock = new Mock<ITransactionService>();
        mock.Setup(ts => ts.DeleteTransactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    internal static RuleParameterCreateRequest MerchantSetAction(string value = "updated") =>
        new()
        {
            Field = AutomaticRuleConstants.TransactionFields.Merchant,
            Operator = AutomaticRuleConstants.ActionOperators.Set,
            Value = value,
        };
}
