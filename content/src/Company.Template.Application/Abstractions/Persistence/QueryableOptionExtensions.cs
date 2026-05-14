namespace Company.Template.Application.Abstractions.Persistence;

public static class QueryableOptionExtensions
{
    extension<T>(IQueryable<T> query) where T : class
    {
        public async Task<Option<T>> FirstOrNoneAsync(CancellationToken cancellationToken = default)
        {
            T? entity = await query.FirstOrDefaultAsync(cancellationToken);

            return Option.FromNullable(entity);
        }

        public async Task<Option<T>> SingleOrNoneAsync(CancellationToken cancellationToken = default)
        {
            T? entity = await query.SingleOrDefaultAsync(cancellationToken);

            return Option.FromNullable(entity);
        }
    }
}
