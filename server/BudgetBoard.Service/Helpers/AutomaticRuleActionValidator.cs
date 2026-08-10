using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Service.Helpers;

internal static class AutomaticRuleActionValidator
{
    internal static bool IsTagOperator(string actionOperator)
    {
        return actionOperator.Equals(
                AutomaticRuleConstants.ActionOperators.Add,
                StringComparison.CurrentCultureIgnoreCase
            )
            || actionOperator.Equals(
                AutomaticRuleConstants.ActionOperators.Remove,
                StringComparison.CurrentCultureIgnoreCase
            );
    }

    internal static void Validate(
        IEnumerable<IRuleParameterRequest> actions,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        foreach (var action in actions)
        {
            if (
                action.Operator.Equals(
                    AutomaticRuleConstants.ActionOperators.Delete,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                continue;
            }

            if (
                action.Field.Equals(
                    AutomaticRuleConstants.TransactionFields.Note,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                if (
                    !action.Operator.Equals(
                        AutomaticRuleConstants.ActionOperators.Set,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                )
                {
                    throw InvalidCombination(action, responseLocalizer);
                }

                continue;
            }

            if (
                action.Field.Equals(
                    AutomaticRuleConstants.TransactionFields.Tags,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                if (!IsTagOperator(action.Operator))
                {
                    throw InvalidCombination(action, responseLocalizer);
                }

                ParseTags(action, responseLocalizer);
                continue;
            }

            if (
                action.Field.Equals(
                    AutomaticRuleConstants.TransactionFields.Amount,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                if (
                    !action.Operator.Equals(
                        AutomaticRuleConstants.ActionOperators.Set,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                )
                {
                    throw InvalidCombination(action, responseLocalizer);
                }

                ParseAmountExpression(action, responseLocalizer);
                continue;
            }

            if (IsTagOperator(action.Operator))
            {
                throw InvalidCombination(action, responseLocalizer);
            }
        }
    }

    internal static AmountExpression ParseAmountExpression(
        IRuleParameterRequest action,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        try
        {
            return AmountExpressionParser.Parse(action.Value);
        }
        catch (AmountExpressionException exception)
        {
            throw CreateExpressionException(action, exception, responseLocalizer);
        }
    }

    internal static IReadOnlyList<string> ParseTags(
        IRuleParameterRequest action,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(action.Value);
        }
        catch (JsonException)
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidTagsError"]
            );
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutomaticRuleInvalidTagsError"]
                );
            }

            var tags = new List<string>();
            foreach (var tagElement in document.RootElement.EnumerateArray())
            {
                if (tagElement.ValueKind != JsonValueKind.String)
                {
                    throw new BudgetBoardServiceException(
                        responseLocalizer["AutomaticRuleInvalidTagsError"]
                    );
                }

                var tag = tagElement.GetString();
                if (string.IsNullOrWhiteSpace(tag))
                {
                    throw new BudgetBoardServiceException(responseLocalizer["TagValueEmptyError"]);
                }

                if (tag.Trim().Length > Tag.MaxValueLength)
                {
                    throw new BudgetBoardServiceException(
                        responseLocalizer["TagValueTooLongError"]
                    );
                }

                tags.Add(tag);
            }

            if (tags.Count == 0)
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutomaticRuleInvalidTagsError"]
                );
            }

            return tags;
        }
    }

    private static BudgetBoardServiceException InvalidCombination(
        IRuleParameterRequest action,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        return new BudgetBoardServiceException(
            responseLocalizer[
                "AutomaticRuleInvalidActionCombinationError",
                action.Field,
                action.Operator
            ]
        );
    }

    internal static BudgetBoardServiceException CreateExpressionException(
        IRuleParameterRequest action,
        AmountExpressionException exception,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        return exception.Error switch
        {
            AmountExpressionError.DivisionByZero => new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleDivisionByZeroError"]
            ),
            AmountExpressionError.Overflow => new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleArithmeticOverflowError"]
            ),
            _ => new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidAmountExpressionError", action.Value]
            ),
        };
    }
}
