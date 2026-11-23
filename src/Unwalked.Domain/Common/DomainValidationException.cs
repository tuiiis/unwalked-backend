namespace Unwalked.Domain.Common;

/// <summary>
/// Thrown when input breaks a domain rule. Mapped to 400 with the message under the field name.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string field, string message)
        : base(message)
    {
        Field = field;
    }

    /// <summary>
    /// Name of the request field the error belongs to, as the client sent it.
    /// </summary>
    public string Field { get; }
}
