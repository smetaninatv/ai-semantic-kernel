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
    private readonly Kernel _kernel;
    private readonly ILogger<SQLExecutionPlugin>? _logger;

    private readonly IProductRepository<Product> _product;

    public SQLExecutionPlugin(IConfiguration configuration, IProductRepository<Product> product, ILogger<SQLExecutionPlugin>? logger)
    {
        var openAIConfig = configuration?.GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>();

        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(openAIConfig!.Deployment, openAIConfig.Endpoint, openAIConfig.ApiKey)
            .Build();

        _product = product;
        _logger = logger;
    }

    [KernelFunction("execute_sql")]
    [Description("Executes SELECT query on  PostgreSQL.")]
    [return: Description("Executes SELECT query on  PostgreSQL. Provides result as json.")]
    public async Task<string> ExecuteSqlAsync(KernelArguments arguments)
    {
        string? sqlCommand = arguments["message"]?.ToString();

        var resultAsString = string.Empty;
        try
        {
            List<Product>? products = await _product.GetListAsync(sqlCommand ?? "");

            resultAsString = JsonConvert.SerializeObject(products);
        }
        catch (Exception exception)
        {
            _logger?.LogError("ChatAsync", exception);
        }

        string history = $"{arguments["history"]}";
        history += $"\nUser: {arguments["message"]}\nSuggestions: {resultAsString}\n";
        return history;
    }
}