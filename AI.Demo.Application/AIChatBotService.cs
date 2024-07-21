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

        // Execute plugins

        // Generate SQL query
        string? pluginResult = await ExecutePluginFunction(kernel, "SQLMessagePlugin", "generate_sql", history, userRequest.Message, openAIPromptExecutionSettings);
        history.AddSystemMessage(@$"Here is the sql query to get data from the database: {pluginResult}");

        // Execute SQL query
        pluginResult = await ExecutePluginFunction(kernel, "SQLExecutionPlugin", "execute_sql", history, pluginResult, openAIPromptExecutionSettings);
        history.AddSystemMessage(@$"Here is the information from the database for the prompt '{userRequest.Message}' in json format: {pluginResult}");

        //pluginResult = await ExecutePluginFunction(kernel, "ResultConverterPlugin", "convert_sql", history, pluginResult, openAIPromptExecutionSettings);
        //history.AddSystemMessage(@$"Here is the text representation of json: {pluginResult}");

        // Convert json to the user firendly message
        history.AddSystemMessage(@$"Use the json content and the data from it to answer the question. Make the response friendly and polite.");

        var result = await _chatCompletionService.GetChatMessageContentAsync(
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

    private async Task<string?> ExecutePluginFunction(Kernel kernel, string pluginName, string functionName, ChatHistory history, string message, OpenAIPromptExecutionSettings? executionSettings) 
    {
        KernelArguments arguments = new KernelArguments(executionSettings);
        arguments.Add("history", history);
        arguments.Add("message", message);

        var plugin = await kernel.Plugins.GetFunction(pluginName, functionName).InvokeAsync(kernel, arguments);
        return plugin.GetValue<string>();
    }
}
