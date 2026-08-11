using System.Globalization;

namespace BudgetBoard.Service.Helpers;

internal enum AmountExpressionError
{
    Invalid,
    DivisionByZero,
    Overflow,
}

internal sealed class AmountExpressionException(AmountExpressionError error) : Exception
{
    internal AmountExpressionError Error { get; } = error;
}

internal sealed class AmountExpression
{
    private readonly AmountExpressionParser.Node root;

    internal AmountExpression(AmountExpressionParser.Node root)
    {
        this.root = root;
    }

    internal decimal Evaluate(decimal amount)
    {
        return root.Evaluate(amount);
    }
}

internal static class AmountExpressionParser
{
    private const int MaximumExpressionLength = 256;
    private const int MaximumTokenCount = 128;

    internal static AmountExpression Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression) || expression.Length > MaximumExpressionLength)
        {
            throw new AmountExpressionException(AmountExpressionError.Invalid);
        }

        try
        {
            var parser = new Parser(expression);
            return new AmountExpression(parser.Parse());
        }
        catch (AmountExpressionException)
        {
            throw;
        }
        catch (OverflowException)
        {
            throw new AmountExpressionException(AmountExpressionError.Overflow);
        }
    }

    internal abstract class Node
    {
        internal abstract decimal Evaluate(decimal amount);

        internal abstract bool TryGetConstant(out decimal value);
    }

    private enum BinaryOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    private sealed class Parser(string expression)
    {
        private readonly string expression = expression;
        private int position;
        private int tokenCount;

        internal Node Parse()
        {
            var result = ParseExpression();
            SkipWhitespace();
            if (position != expression.Length)
            {
                throw new AmountExpressionException(AmountExpressionError.Invalid);
            }

            return result;
        }

        private Node ParseExpression()
        {
            var result = ParseTerm();
            while (true)
            {
                SkipWhitespace();
                if (!TryConsume('+') && !TryConsume('-'))
                {
                    return result;
                }

                var operation =
                    expression[position - 1] == '+'
                        ? BinaryOperation.Add
                        : BinaryOperation.Subtract;
                var right = ParseTerm();
                result = new BinaryNode(operation, result, right);
            }
        }

        private Node ParseTerm()
        {
            var result = ParseUnary();
            while (true)
            {
                SkipWhitespace();
                if (!TryConsume('*') && !TryConsume('/'))
                {
                    return result;
                }

                var operation =
                    expression[position - 1] == '*'
                        ? BinaryOperation.Multiply
                        : BinaryOperation.Divide;
                var right = ParseUnary();
                result = new BinaryNode(operation, result, right);
            }
        }

        private Node ParseUnary()
        {
            SkipWhitespace();
            if (TryConsume('+') || TryConsume('-'))
            {
                return new UnaryNode(expression[position - 1], ParseUnary());
            }

            return ParsePrimary();
        }

        private Node ParsePrimary()
        {
            SkipWhitespace();
            if (TryConsume('('))
            {
                var result = ParseExpression();
                SkipWhitespace();
                if (!TryConsume(')'))
                {
                    throw new AmountExpressionException(AmountExpressionError.Invalid);
                }

                return result;
            }

            if (position < expression.Length && IsAsciiDigit(expression[position]))
            {
                return ParseNumber();
            }

            if (position < expression.Length && IsIdentifierStart(expression[position]))
            {
                return ParseIdentifier();
            }

            throw new AmountExpressionException(AmountExpressionError.Invalid);
        }

        private Node ParseNumber()
        {
            var start = position;
            while (position < expression.Length && IsAsciiDigit(expression[position]))
            {
                position++;
            }

            if (position < expression.Length && expression[position] == '.')
            {
                position++;
                while (position < expression.Length && IsAsciiDigit(expression[position]))
                {
                    position++;
                }
            }

            var literal = expression[start..position];
            if (
                !decimal.TryParse(
                    literal,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var value
                )
            )
            {
                throw new AmountExpressionException(AmountExpressionError.Overflow);
            }

            CountToken();
            return new LiteralNode(value);
        }

        private Node ParseIdentifier()
        {
            var start = position;
            while (
                position < expression.Length
                && (IsIdentifierStart(expression[position]) || IsAsciiDigit(expression[position]))
            )
            {
                position++;
            }

            var identifier = expression[start..position];
            if (!identifier.Equals("amount", StringComparison.OrdinalIgnoreCase))
            {
                throw new AmountExpressionException(AmountExpressionError.Invalid);
            }

            CountToken();
            return new AmountNode();
        }

        private bool TryConsume(char expected)
        {
            if (position >= expression.Length || expression[position] != expected)
            {
                return false;
            }

            position++;
            CountToken();
            return true;
        }

        private void SkipWhitespace()
        {
            while (position < expression.Length && char.IsWhiteSpace(expression[position]))
            {
                position++;
            }
        }

        private void CountToken()
        {
            tokenCount++;
            if (tokenCount > MaximumTokenCount)
            {
                throw new AmountExpressionException(AmountExpressionError.Invalid);
            }
        }

        private static bool IsAsciiDigit(char value)
        {
            return value is >= '0' and <= '9';
        }

        private static bool IsIdentifierStart(char value)
        {
            return value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
        }
    }

    private sealed class LiteralNode(decimal value) : Node
    {
        internal override decimal Evaluate(decimal amount)
        {
            return value;
        }

        internal override bool TryGetConstant(out decimal constant)
        {
            constant = value;
            return true;
        }
    }

    private sealed class AmountNode : Node
    {
        internal override decimal Evaluate(decimal amount)
        {
            return amount;
        }

        internal override bool TryGetConstant(out decimal constant)
        {
            constant = default;
            return false;
        }
    }

    private sealed class UnaryNode(char operation, Node operand) : Node
    {
        internal override decimal Evaluate(decimal amount)
        {
            return operation == '-' ? checked(-operand.Evaluate(amount)) : operand.Evaluate(amount);
        }

        internal override bool TryGetConstant(out decimal constant)
        {
            if (!operand.TryGetConstant(out var value))
            {
                constant = default;
                return false;
            }

            constant = operation == '-' ? checked(-value) : value;
            return true;
        }
    }

    private sealed class BinaryNode : Node
    {
        private readonly BinaryOperation operation;
        private readonly Node left;
        private readonly Node right;

        internal BinaryNode(BinaryOperation operation, Node left, Node right)
        {
            this.operation = operation;
            this.left = left;
            this.right = right;

            if (
                operation == BinaryOperation.Divide
                && right.TryGetConstant(out var divisor)
                && divisor == 0
            )
            {
                throw new AmountExpressionException(AmountExpressionError.DivisionByZero);
            }
        }

        internal override decimal Evaluate(decimal amount)
        {
            var leftValue = left.Evaluate(amount);
            var rightValue = right.Evaluate(amount);
            if (operation == BinaryOperation.Add)
            {
                return checked(leftValue + rightValue);
            }
            if (operation == BinaryOperation.Subtract)
            {
                return checked(leftValue - rightValue);
            }
            if (operation == BinaryOperation.Multiply)
            {
                return checked(leftValue * rightValue);
            }
            return leftValue / rightValue;
        }

        internal override bool TryGetConstant(out decimal constant)
        {
            if (!left.TryGetConstant(out _) || !right.TryGetConstant(out _))
            {
                constant = default;
                return false;
            }

            constant = Evaluate(0);
            return true;
        }
    }
}
