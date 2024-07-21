using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;

namespace AI.Demo.Application;

public class ResultConverterPlugin
{
    private readonly Kernel _kernel;
    private readonly IPromptTemplateFactory _promptTemplateFactory;
    private readonly ILogger<ResultConverterPlugin>? _logger;

    public ResultConverterPlugin(IConfiguration configuration, ILogger<ResultConverterPlugin>? logger)
    {
        var openAIConfig = configuration?.GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>();

        _kernel = Kernel.CreateBuilder()
           .AddAzureOpenAIChatCompletion(openAIConfig!.Deployment, openAIConfig.Endpoint, openAIConfig.ApiKey)
           .Build();

        _promptTemplateFactory = new KernelPromptTemplateFactory();
        _logger = logger;
    }

    [KernelFunction("convert_sql")]
    [Description("Converts SQL result to the user friendly message.")]
    [return: Description("Converts SQL result to the user friendly message.")]
    public async Task<string> ConverToUserFriendlyMessageAsync(KernelArguments arguments)
    {
        var prompt = @"The history of user request:
            {{$history}}

            Use provided json content and the data from it to answer the question.
            {{$message}}

            ChatBot:";

        var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(prompt)).RenderAsync(_kernel, arguments);

        var function = _kernel.CreateFunctionFromPrompt(
            promptTemplate: renderedPrompt,
            functionName: nameof(ConverToUserFriendlyMessageAsync),
            description: "Complete the prompt.");

        var result = string.Empty;
        try
        {
            var invokeResult = await function.InvokeAsync(_kernel, arguments);
            result = invokeResult.GetValue<string>();
        }
        catch(Exception exception)
        {
            _logger?.LogError("ConverToUserFriendlyMessageAsync", exception);
        }

        return result;
    }

}