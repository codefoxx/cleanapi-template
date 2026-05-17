namespace Company.Template.Application.Common;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
        where T : notnull
    {
        public Result<TResult> Map<TResult>(Func<T, TResult> map)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(map);

            return result.IsSuccess
                ? Result<TResult>.Success(map(result.Value))
                : Result<TResult>.Failure(result.Error);
        }

        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> bind)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess
                ? bind(result.Value)
                : Result<TResult>.Failure(result.Error);
        }

        public async Task<Result<TResult>> BindAsync<TResult>(
            Func<T, Task<Result<TResult>>> bind)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess
                ? await bind(result.Value)
                : Result<TResult>.Failure(result.Error);
        }
    }
}
