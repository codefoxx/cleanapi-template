using Company.Template.Application.Common;

namespace Company.Template.Application.Abstractions;

public interface IUseCase<in TRequest, TResult>
{
    Task<Result<TResult>> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IUseCase<in TRequest>
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}
