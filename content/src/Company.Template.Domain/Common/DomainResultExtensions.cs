namespace Company.Template.Domain.Common;

public static class DomainResultExtensions
{
    extension<T>(DomainResult<T> result) where T : notnull
    {
        public DomainResult<TResult> Map<TResult>(Func<T, TResult> map)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(map);

            return result.IsSuccess
                ? DomainResult<TResult>.Success(map(result.Value))
                : DomainResult<TResult>.Failure(result.Error);
        }

        public DomainResult<TResult> Bind<TResult>(Func<T, DomainResult<TResult>> bind)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess
                ? bind(result.Value)
                : DomainResult<TResult>.Failure(result.Error);
        }
    }
}
