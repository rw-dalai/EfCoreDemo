namespace Conversions.Models;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new ArgumentException($"'{value}' is not a valid email.");

        Value = value.Trim();
    }
}
