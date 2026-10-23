using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.BusinessLogic.Abstractions;
using RestaurantCRM.DataAccess.Data;

namespace RestaurantCRM.DataAccess.Repositories;

public sealed class EfRepository(RestaurantCrmContext context) : IRepository
{
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class => context.Set<TEntity>();
    public Task<List<TResult>> ListAsync<TEntity, TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) where TEntity : class =>
        context.Set<TEntity>().Where(predicate).Select(selector).ToListAsync();
    public Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class => context.Set<TEntity>().AnyAsync(predicate);
    public ValueTask<TEntity?> FindAsync<TEntity>(params object?[] keyValues) where TEntity : class => context.Set<TEntity>().FindAsync(keyValues);
    public void Add<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Add(entity);
    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => context.Set<TEntity>().AddRange(entities);
    public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Remove(entity);
    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => context.Set<TEntity>().RemoveRange(entities);
    public void Update<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Update(entity);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new EfAppTransaction(await context.Database.BeginTransactionAsync(cancellationToken));

    public Task LoadReferenceAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> navigation, CancellationToken cancellationToken = default)
        where TEntity : class where TProperty : class => context.Entry(entity).Reference(navigation).LoadAsync(cancellationToken);

    private sealed class EfAppTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : IAppTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);
        public Task RollbackAsync(CancellationToken cancellationToken = default) => transaction.RollbackAsync(cancellationToken);
        public void Dispose() => transaction.Dispose();
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
