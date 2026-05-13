using Company.Template.Application.Common;

namespace Company.Template.Application.Abstractions;

/// <summary>
///     Defines the contract for an application use case that returns a result.
/// </summary>
/// <remarks>
///     Use cases coordinate application operations and invoke domain behavior to fulfill
///     a user request. They return a <see cref="Result{T}" /> to handle expected business
///     failures gracefully without relying on exceptions.
/// </remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResult">The type of the result value.</typeparam>
public interface IUseCase<in TRequest, TResult>
{
    Task<Result<TResult>> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
///     Defines the contract for an application use case that returns a result with no value.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public interface IUseCase<in TRequest>
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
