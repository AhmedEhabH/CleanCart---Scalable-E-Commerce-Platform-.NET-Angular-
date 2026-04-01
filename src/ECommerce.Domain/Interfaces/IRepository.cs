using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IReadRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetBySpecAsync<ISpec>(ISpec spec, CancellationToken cancellationToken = default) where ISpec : ISpecification<T>;
    Task<IReadOnlyList<T>> GetAllBySpecAsync<ISpec>(ISpec spec, CancellationToken cancellationToken = default) where ISpec : ISpecification<T>;
    Task<int> CountAsync<ISpec>(ISpec spec, CancellationToken cancellationToken = default) where ISpec : ISpecification<T>;
}

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public interface IQueryRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
