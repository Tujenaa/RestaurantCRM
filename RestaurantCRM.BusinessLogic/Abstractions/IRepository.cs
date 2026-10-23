using System.Linq.Expressions;

namespace RestaurantCRM.BusinessLogic.Abstractions;

public interface IAppTransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IRepository
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    Task<List<TResult>> ListAsync<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) where TEntity : class;
    Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
    ValueTask<TEntity?> FindAsync<TEntity>(params object?[] keyValues) where TEntity : class;
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task LoadReferenceAsync<TEntity, TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty?>> navigation,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TProperty : class;
}
