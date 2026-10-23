using RestaurantCRM.BusinessLogic.Abstractions;

namespace RestaurantCRM.BusinessLogic.Services;

public interface IRestaurantService : IRepository
{
}

public sealed class RestaurantService(IRepository repository) : IRestaurantService
{
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class => repository.Query<TEntity>();
    public Task<List<TResult>> ListAsync<TEntity, TResult>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, System.Linq.Expressions.Expression<Func<TEntity, TResult>> selector) where TEntity : class => repository.ListAsync(predicate, selector);
    public Task<bool> AnyAsync<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class => repository.AnyAsync(predicate);
    public ValueTask<TEntity?> FindAsync<TEntity>(params object?[] keyValues) where TEntity : class => repository.FindAsync<TEntity>(keyValues);
    public void Add<TEntity>(TEntity entity) where TEntity : class => repository.Add(entity);
    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => repository.AddRange(entities);
    public void Remove<TEntity>(TEntity entity) where TEntity : class => repository.Remove(entity);
    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => repository.RemoveRange(entities);
    public void Update<TEntity>(TEntity entity) where TEntity : class => repository.Update(entity);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => repository.SaveChangesAsync(cancellationToken);
    public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => repository.BeginTransactionAsync(cancellationToken);
    public Task LoadReferenceAsync<TEntity, TProperty>(TEntity entity, System.Linq.Expressions.Expression<Func<TEntity, TProperty?>> navigation, CancellationToken cancellationToken = default)
        where TEntity : class where TProperty : class => repository.LoadReferenceAsync(entity, navigation, cancellationToken);
}
