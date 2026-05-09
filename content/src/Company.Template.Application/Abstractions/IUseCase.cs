using Company.Template.Application.Common;

namespace Company.Template.Application.Abstractions;

internal interface IUseCase<in TCommand, TResult>
{
    Task<Result<TResult>> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}

internal interface IUseCase<in TCommand>
{
    Task<Result> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
