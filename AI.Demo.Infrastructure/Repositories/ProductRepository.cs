using AI.Demo.Infrastructure.Connection;
using Dapper;
using Npgsql;

namespace AI.Demo.Infrastructure.Repositories;

public class ProductRepository<T> : IProductRepository<T>
{
    private readonly IConnectionStringProvider _connection;

    public ProductRepository(IConnectionStringProvider connection)
    {
        _connection = connection;
    }

    public async Task<List<T>?> GetListAsync(string sql, CancellationToken cancellationToken = default)
    {
        using var connection = new NpgsqlConnection(_connection.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        IEnumerable<T> response = await connection.QueryAsync<T>(sql);

        return response?.ToList();
    } 
}
