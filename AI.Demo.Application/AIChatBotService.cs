using AI.Demo.Domain.Models;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using AI.Demo.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Planning.Handlebars;

namespace AI.Demo.Application;

#pragma warning disable SKEXP0060
public class AIChatBotService(AzureOpenAISettings _openAIConfig, OpenAIPromptSettings _promptConfig, 
    SQLMessagePlugin sqlMessagePlugin, SQLExecutionPlugin sqlExecutionPlugin, ResultConverterPlugin resultConverterPlugin, ILogger<AIChatBotService> _logger) : IAIChatBotService
{
    public async Task<AIResponse> GetAnswer(UserRequest userRequest) 
    {
        // Init Semantic Kernel
        var kernel = InitKernel();

        // Enable plugins
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            TopP = _promptConfig.TopP,
            Temperature = _promptConfig.Temperature,
            MaxTokens = _promptConfig.MaxTokens,
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };

        // Execute plugins
        var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions()
        {
            ExecutionSettings = openAIPromptExecutionSettings,
        });

        var ask = @$"Use the SQLMessagePlugin, SQLExecutionPlugin, and ResultConverterPlugin to get an answer from our database.
                    Analyze the JSON data from the plugin execution results and respond to the prompt.";
        
        var arguments = new KernelArguments()
        {
            { "message", $"{userRequest.UserName}: {userRequest.Message}" },
            { "execution_result", "" },
        };

        var plan = await planner.CreatePlanAsync(kernel, ask, arguments);
        
        var result = await plan.InvokeAsync(kernel, arguments);

        return new AIResponse
        {
            Date = DateTime.Now,
            UserRequest = userRequest,
            Message = result ?? ""
        };
    }

    private Kernel InitKernel()
    {
        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(_openAIConfig.Deployment, _openAIConfig.Endpoint, _openAIConfig.ApiKey);

        builder.Plugins.AddFromObject(sqlMessagePlugin);
        builder.Plugins.AddFromObject(sqlExecutionPlugin);
        builder.Plugins.AddFromObject(resultConverterPlugin);

        return builder.Build();
    }
}
