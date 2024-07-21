using AI.Demo.Domain.Models;
using AI.Demo.Infrastructure;
using AI.Demo.Infrastructure.Repositories;
using Google.Apis.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AI.Demo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        
        var openAIConfig = configuration?.GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>();
        services.AddSingleton<AzureOpenAISettings>(openAIConfig!);
        
        var promptConfig = configuration?.GetSection(nameof(OpenAIPromptSettings)).Get<OpenAIPromptSettings>();
        services.AddSingleton<OpenAIPromptSettings>(promptConfig!);

        services.AddTransient<SQLMessagePlugin>();
        services.AddTransient<SQLExecutionPlugin>();
        services.AddTransient<ResultConverterPlugin>();

        // Create native plugin collection
        services.AddTransient((serviceProvider) => {
            KernelPluginCollection pluginCollection = [];
            
            return pluginCollection;
        });

        // Create the kernel service
        services.AddTransient<Kernel>((serviceProvider) => {
            
            KernelPluginCollection pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();

            return new Kernel(serviceProvider, pluginCollection);
        });

        return services;
    }
}
