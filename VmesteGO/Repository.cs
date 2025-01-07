using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using VmesteGO.Domain.Entities;
using VmesteGO.Exceptions;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace VmesteGO;

public class Repository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity>
	where TEntity : BaseEntity
{
	public Repository(ApplicationDbContext context) : base(context)
	{
		Context = context;
		Set = Context.Set<TEntity>();
	}

	protected ApplicationDbContext Context { get; }

	protected DbSet<TEntity> Set { get; }

	public async Task<bool> TryAddUniqueAsync(TEntity entity)
	{
		try
		{
			await AddAsync(entity);
			return true;
		}
		catch (UniqueConstraintException)
		{
			return false;
		}
	}

	public async Task<TEntity> FirstAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
	{
		return await FirstOrDefaultAsync(specification, cancellationToken)
			?? throw new EntityNotFoundException<TEntity>(specification);
	}

	public async Task<TResult> FirstAsync<TResult>(ISpecification<TEntity, TResult> specification,
		CancellationToken cancellationToken = default)
	{
		return await FirstOrDefaultAsync(specification, cancellationToken)
			?? throw new EntityNotFoundException<TEntity>(specification);
	}

	async Task<TEntity> IRepository<TEntity>.GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken)
	{
		return await FindByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException<TEntity>(id.ToString()!);
	}

	public async Task<TEntity?> FindByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
	{
		return await base.GetByIdAsync(id, cancellationToken);
	}

	public async Task<TEntity?> LastOrDefaultAsync(ISpecification<TEntity> specification,
		CancellationToken cancellationToken = default)
	{
		return await ApplySpecification(specification).LastOrDefaultAsync(cancellationToken);
	}

	public void Delete(TEntity entity)
	{
		Set.Remove(entity);
	}

	public void DeleteRange(IEnumerable<TEntity> entities)
	{
		Set.RemoveRange(entities);
	}

	public void Add(TEntity entity)
	{
		Set.Add(entity);
	}

	public void AddRange(IEnumerable<TEntity> entities)
	{
		Set.AddRange(entities);
	}
}