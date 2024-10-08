using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using AI.Demo.Infrastructure.Repositories;
using AI.Demo.Domain.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace AI.Demo.Application;

public class SQLExecutionPlugin
{
    private readonly ILogger<SQLExecutionPlugin>? _logger;

    private readonly IProductRepository<object> _product;

    public SQLExecutionPlugin(IConfiguration configuration, IProductRepository<object> product, ILogger<SQLExecutionPlugin>? logger)
    {
        _product = product;
        _logger = logger;
    }

    [KernelFunction("execute_sql")]
    [Description("Executes SELECT query on  PostgreSQL.")]
    [return: Description("Executes SELECT query on  PostgreSQL. Provides result as json.")]
    public async Task<string> ExecuteSqlAsync(KernelArguments arguments)
    {
        string? sqlCommand = arguments["execution_result"]?.ToString();

        var result = string.Empty;
        try
        {
            List<object>? products = await _product.GetListAsync(sqlCommand ?? "");

            result = JsonConvert.SerializeObject(products);

            arguments["execution_result"] = result;
        }
        catch (Exception exception)
        {
            _logger?.LogError("ExecuteSqlAsync", exception);
            return $"Could not execute given SQL request in PostgreSQL database. Please, try again. Error: {exception.Message}";
        }

        return result;
    }
}