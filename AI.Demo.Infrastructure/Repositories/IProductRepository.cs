using AI.Demo.Domain.Models;

namespace AI.Demo.Infrastructure.Repositories;

public interface IProductRepository<T>
{
    Task<List<T>?> GetListAsync(string sql, CancellationToken cancellationToken = default);
}