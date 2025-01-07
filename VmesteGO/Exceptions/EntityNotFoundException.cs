using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}

public sealed class EntityNotFoundException<TEntity> : Exception
    where TEntity : BaseEntity
{
    public EntityNotFoundException(string id) : base($"{typeof(TEntity)} with id '{id}' not found")
    {
    }

    public EntityNotFoundException(int id) : this(id.ToString())
    {
    }

    public EntityNotFoundException(ISpecification<TEntity> specification)
        : base($"Entity of type '{typeof(TEntity)}' by specification '{specification.GetType()}' not found'")
    {
    }
}