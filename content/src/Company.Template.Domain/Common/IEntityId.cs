namespace Company.Template.Domain.Common;

/// <summary>
///     Marks a strongly typed entity identifier.
/// </summary>
/// <typeparam name="TSelf">The concrete identifier type.</typeparam>
public interface IEntityId<TSelf>
    where TSelf : struct, IEntityId<TSelf>
{
    /// <summary>
    ///     Gets the underlying identifier value.
    /// </summary>
    Guid Value { get; }
}

//( <summary>
