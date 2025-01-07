namespace VmesteGO.Domain.Entities;

public abstract class BaseEntity
{
}

public abstract class BaseEntity<TKey> : BaseEntity
{
    public TKey Id { get; set; } = default!;
}