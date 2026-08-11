using System.Globalization;
using BudgetBoard.Service.Helpers;
using FluentAssertions;

namespace BudgetBoard.IntegrationTests;

public class AmountExpressionParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    public void Parse_EmptyExpression_ThrowsInvalidExpression(string expression)
    {
        var act = () => AmountExpressionParser.Parse(expression);

        act.Should()
            .Throw<AmountExpressionException>()
            .Which.Error.Should()
            .Be(AmountExpressionError.Invalid);
    }

    [Fact]
    public void Parse_ExpressionLongerThanMaximum_ThrowsInvalidExpression()
    {
        var act = () => AmountExpressionParser.Parse(new string('1', 257));

        act.Should()
            .Throw<AmountExpressionException>()
            .Which.Error.Should()
            .Be(AmountExpressionError.Invalid);
    }

    [Theory]
    [InlineData("1 2")]
    [InlineData("*1")]
    [InlineData("_amount")]
    [InlineData("amount2")]
    [InlineData("{1")]
    [InlineData("[1")]
    public void Parse_InvalidSyntax_ThrowsInvalidExpression(string expression)
    {
        var act = () => AmountExpressionParser.Parse(expression);

        act.Should()
            .Throw<AmountExpressionException>()
            .Which.Error.Should()
            .Be(AmountExpressionError.Invalid);
    }

    [Fact]
    public void Parse_MoreThanMaximumTokens_ThrowsInvalidExpression()
    {
        var expression = string.Join("+", Enumerable.Repeat("1", 65));
        var act = () => AmountExpressionParser.Parse(expression);

        act.Should()
            .Throw<AmountExpressionException>()
            .Which.Error.Should()
            .Be(AmountExpressionError.Invalid);
    }

    [Theory]
    [InlineData("+amount", "2")]
    [InlineData("1 / +1", "1")]
    [InlineData("1 / -1", "-1")]
    [InlineData("1 / -amount", "-0.5")]
    [InlineData("1 / (2 + 2)", "0.25")]
    [InlineData("1 / (AMOUNT + 1)", "0.3333333333333333333333333333")]
    public void Parse_SupportedExpression_EvaluatesCorrectly(string expression, string expected)
    {
        var parsed = AmountExpressionParser.Parse(expression);

        parsed.Evaluate(2m).Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("1 / 0", "DivisionByZero")]
    [InlineData("79228162514264337593543950336", "Overflow")]
    [InlineData("1 / (79228162514264337593543950335 + 1)", "Overflow")]
    public void Parse_InvalidArithmetic_ThrowsExpectedError(string expression, string expectedError)
    {
        var act = () => AmountExpressionParser.Parse(expression);

        act.Should()
            .Throw<AmountExpressionException>()
            .Which.Error.Should()
            .Be(Enum.Parse<AmountExpressionError>(expectedError));
    }
}
