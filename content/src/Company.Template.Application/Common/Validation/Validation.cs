using System.Linq.Expressions;

namespace Company.Template.Application.Common.Validation;

/// <summary>
///     Entry point for lightweight application/request validation.
/// </summary>
/// <remarks>
///     This validation helper intentionally stays small and dependency-free. It is used at the
///     application/API boundary to collect independent request validation errors before a command
///     or query is created.
///     <para>
///         Use <see cref="For{T}(T)" /> instead of creating <see cref="ValidationBuilder{T}" />
///         directly. The builder constructor is internal to keep the intended creation path explicit
///         while still keeping the fluent API simple.
///     </para>
///     <para>
///         Unlike <c>Bind</c>-based result composition, this validation flow does not fail fast.
///         Every configured rule is executed so the API can return all field-level validation errors
///         in one response.
///     </para>
/// </remarks>
public static class Validation
{
    /// <summary>
    ///     Starts validation for the supplied value.
    /// </summary>
    /// <typeparam name="T">The request or input model type being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <returns>A validation builder for adding request-level or property-level rules.</returns>
    public static ValidationBuilder<T> For<T>(T value)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ValidationBuilder<T>(value);
    }
}

/// <summary>
///     Collects validation errors for a single request or input value.
/// </summary>
/// <typeparam name="T">The request or input model type being validated.</typeparam>
/// <remarks>
///     The builder is mutable by design and should be used only as a short-lived fluent object inside
///     validation methods. Each rule is evaluated immediately and any returned error is collected.
///     Mapping to a result is deferred until <see cref="Map{TResult}(Func{T, TResult})" /> is called.
/// </remarks>
public sealed class ValidationBuilder<T>
    where T : notnull
{
    private readonly List<Error> _errors = [];
    private readonly T _value;

    /// <summary>
    ///     Creates a validation builder for the supplied value.
    /// </summary>
    /// <remarks>
    ///     This constructor is internal to discourage direct construction. Consumers should start
    ///     validation through <see cref="Validation.For{T}(T)" />, which documents the intended usage
    ///     and keeps the validation API consistent.
    /// </remarks>
    internal ValidationBuilder(T value)
    {
        _value = value;
    }

    /// <summary>
    ///     Adds a rule that validates the complete input value.
    /// </summary>
    /// <param name="rule">
    ///     A validation rule that returns an <see cref="Error" /> when the rule fails;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>The same builder instance so additional rules can be chained.</returns>
    /// <remarks>
    ///     Use this overload for cross-field rules or validations that need access to the whole
    ///     request. Use <see cref="RuleFor{TProperty}" /> for single-property validation.
    /// </remarks>
    public ValidationBuilder<T> Rule(Func<T, Error?> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        Error? error = rule(_value);

        if (error is not null)
        {
            _errors.Add(error);
        }

        return this;
    }

    /// <summary>
    ///     Adds a rule that validates a single selected property.
    /// </summary>
    /// <typeparam name="TProperty">The selected property type.</typeparam>
    /// <param name="selector">A simple property access expression.</param>
    /// <param name="rule">
    ///     A validation rule that returns an <see cref="Error" /> when the selected property is invalid;
    ///     otherwise <see langword="null" />.
    /// </param>
    /// <returns>The same builder instance so additional rules can be chained.</returns>
    /// <remarks>
    ///     The property name is extracted from <paramref name="selector" /> and used as the validation
    ///     target when the rule returns an error. This avoids duplicating field names as strings in
    ///     validators while still producing field-level API validation responses.
    ///     <para>
    ///         Only simple property access expressions are supported, for example
    ///         <c>x =&gt; x.Name</c>. More complex expressions should use <see cref="Rule" />.
    ///     </para>
    /// </remarks>
    public ValidationBuilder<T> RuleFor<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Func<TProperty, Error?> rule)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(rule);

        string target = GetTargetName(selector);
        Func<T, TProperty> getProperty = selector.Compile();

        Error? error = rule(getProperty(_value));

        if (error is not null)
        {
            _errors.Add(WithTarget(error, target));
        }

        return this;
    }

    /// <summary>
    ///     Maps the validated input to a result value when all rules passed.
    /// </summary>
    /// <typeparam name="TResult">The mapped result type.</typeparam>
    /// <param name="map">The mapping function used when validation succeeded.</param>
    /// <returns>
    ///     A successful validation result containing the mapped value, or a failed validation result
    ///     containing all collected validation errors.
    /// </returns>
    /// <remarks>
    ///     The mapping function is only executed when no validation errors were collected. This makes
    ///     it safe for the mapper to rely on invariants established by the validation rules.
    /// </remarks>
    public ValidationResult<TResult> Map<TResult>(Func<T, TResult> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);

        return _errors.Count == 0
            ? ValidationResult<TResult>.Success(map(_value))
            : ValidationResult<TResult>.Failure(_errors.ToArray());
    }

    private static Error WithTarget(Error error, string target)
    {
        return error.Target is null
            ? error with { Target = target }
            : error;
    }

    private static string GetTargetName<TProperty>(Expression<Func<T, TProperty>> selector)
    {
        return selector.Body switch
        {
            MemberExpression memberExpression => ToCamelCase(memberExpression.Member.Name),

            UnaryExpression { Operand: MemberExpression memberExpression } =>
                ToCamelCase(memberExpression.Member.Name),

            _ => throw new ArgumentException(
                "Selector must be a simple property access expression.",
                nameof(selector))
        };
    }

    private static string ToCamelCase(string value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }
}
