namespace Phloem.Application.Shared.Validation;

/// <summary>
/// House wording for FluentValidation messages, so every form sounds the same.
/// {PropertyName}, {MaxLength} etc. are filled in by FluentValidation.
/// </summary>
public static class ValidationMessages
{
    public const string Required = "Please enter {PropertyName}.";
    public const string MaxLength = "{PropertyName} must be {MaxLength} characters or fewer.";
    public const string MustBePositive = "{PropertyName} must be more than zero.";
}