using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO;

public interface IRepository<TEntity> : IRepositoryBase<TEntity>
    where TEntity : BaseEntity
{
    Task<bool> TryAddUniqueAsync(TEntity entity);

    Task<TEntity> FirstAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<TResult> FirstAsync<TResult>(ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);

    new Task<TEntity> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

    Task<TEntity?> FindByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;

    Task<TEntity?> LastOrDefaultAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    void Delete(TEntity entity);

    void DeleteRange(IEnumerable<TEntity> entities);
    
    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);
}