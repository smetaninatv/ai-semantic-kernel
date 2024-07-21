using AI.Demo.Domain.Models;
using AI.Demo.Infrastructure.Configuration;
using AI.Demo.Infrastructure.Connection;
using AI.Demo.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Demo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Connections
        services.Configure<PostgreConfig>(configuration.GetSection("PostgreConfig"));

        //Postgre SQL
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();

        //Inject Repositories
        services.AddTransient<IProductRepository<Product>, ProductRepository<Product>>();

        return services;
    }
}
