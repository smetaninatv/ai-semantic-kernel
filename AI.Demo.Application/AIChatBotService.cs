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

namespace AI.Demo.Application;

public class AIChatBotService(AzureOpenAISettings _openAIConfig, OpenAIPromptSettings _promptConfig, 
    SQLMessagePlugin sqlMessagePlugin, SQLExecutionPlugin sqlExecutionPlugin, ResultConverterPlugin resultConverterPlugin, ILogger<AIChatBotService> _logger) : IAIChatBotService
{
    public async Task<AIResponse> GetAnswer(UserRequest userRequest) 
    {
        // Init Semantic Kernel
        var kernel = InitKernel();

        // Create chat history
        var history = new ChatHistory();

        history.AddSystemMessage(@"Use SQLMessagePlugin_generate_sql to generate sql. Return the only sql command.");
        history.AddUserMessage($"{userRequest.UserName}: {userRequest.Message}");

        var _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Enable plugins
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            TopP = _promptConfig.TopP,
            Temperature = _promptConfig.Temperature,
            MaxTokens = _promptConfig.MaxTokens,
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };

        // Get the response from the AI
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel
        );
        
        history.AddAssistantMessage(result?.Content ?? "");

        var plugin = await kernel.Plugins.GetFunction("SQLMessagePlugin", "generate_sql").InvokeAsync(kernel);
        string pluginResult = JsonSerializer.Serialize(plugin);
        history.AddSystemMessage(@$"Use SQLMessagePlugin_generate_sql result: {pluginResult}");

        history.AddSystemMessage(@"Use SQLExecutionPlugin_execute_sql to execute sql. Return json result.");

        result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel
        );

        history.AddAssistantMessage(result?.Content ?? "");

        history.AddSystemMessage(@"Use ResultConverterPlugin_convert_sql to provide user friendly text from json.");

        result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel
        );

        history.AddAssistantMessage(result?.Content ?? "");


        return new AIResponse
        {
            Date = DateTime.Now,
            UserRequest = userRequest,
            Message = result?.Content ?? ""
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
