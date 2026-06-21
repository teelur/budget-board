namespace BudgetBoard.Service.Models;

/// <summary>
/// Represents a tri-state field for update requests:
/// unspecified, explicitly null, or explicitly set value.
/// </summary>
public readonly struct OptionalField<T>
{
    public bool IsSpecified { get; }

    public T? Value { get; }

    public OptionalField(T? value)
    {
        IsSpecified = true;
        Value = value;
    }

    public static implicit operator OptionalField<T>(T? value)
    {
        return new OptionalField<T>(value);
    }
}
