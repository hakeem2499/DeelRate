
namespace DeelRate.Domain.Common;

/// <summary>
/// Represents an error with a code, description, and type.
/// </summary>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new(
        "General.NullValue",
        "Null Value was provided",
        ErrorType.Failure
    );

    /// <summary>
    /// Creates a new instance of <see cref="Error"/> with the <see cref="ErrorType.Failure"/> type.
    /// </summary>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a new instance of <see cref="Error"/> with the <see cref="ErrorType.Problem"/> type.
    /// </summary>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates a new instance of <see cref="Error"/> with the <see cref="ErrorType.NotFound"/> type.
    /// </summary>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a new instance of <see cref="Error"/> with the <see cref="ErrorType.Conflict"/> type.
    /// </summary>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a new instance of <see cref="Error"/> with the <see cref="ErrorType.Validation"/> type.
    /// </summary>
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);
}
