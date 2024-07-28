using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

namespace AI.Demo.Application;

public class SQLMessagePlugin
{
    private readonly Kernel _kernel;
    private readonly IPromptTemplateFactory _promptTemplateFactory;
    private readonly ILogger<SQLMessagePlugin>? _logger;

    public SQLMessagePlugin(IConfiguration configuration, ILogger<SQLMessagePlugin>? logger)
    {
        var openAIConfig = configuration?.GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>();

        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(openAIConfig!.Deployment, openAIConfig.Endpoint, openAIConfig.ApiKey)
            .Build();

        _promptTemplateFactory = new KernelPromptTemplateFactory();
        _logger = logger;
    }

    [KernelFunction("generate_sql")]
    [Description("Generates Postgre SELECT query based on provided user massage.")]
    [return: Description("Generates Postgre SELECT query based on provided user massage.")]
    public async Task<string> GenerateSqlAsync(KernelArguments arguments)
    {
        var prompt = @"Generate Postgre SQL query based on provided user massage.
            Use context below for query generation.
            Return the only SQL query, without additional words or details.

            Database name: ai_demo_products

            It has 3 tables:

            CREATE TABLE public.products
            (
                id serial NOT NULL,
                name character varying(255) COLLATE pg_catalog.""default"" NOT NULL,
                description character varying(255) COLLATE pg_catalog.""default"" NOT NULL,
                supplier_id integer NOT NULL,
                category_id integer NOT NULL,
	            in_stock integer,
                CONSTRAINT products_pkey PRIMARY KEY (id)
            );

            CREATE TABLE IF NOT EXISTS public.suppliers
            (
                id serial NOT NULL,
                name character varying(255) COLLATE pg_catalog.""default"" NOT NULL,
                address character varying(255) COLLATE pg_catalog.""default"",
                CONSTRAINT suppliers_pkey PRIMARY KEY (id)
            );

            CREATE TABLE IF NOT EXISTS public.categories
            (
                id serial NOT NULL,
                name character varying(255) COLLATE pg_catalog.""default"" NOT NULL,
                CONSTRAINT categories_pkey PRIMARY KEY (id)
            );

            {{$message}}

            ChatBot:";

        var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(prompt)).RenderAsync(_kernel, arguments);

        var function = _kernel.CreateFunctionFromPrompt(
            promptTemplate: renderedPrompt,
            functionName: nameof(GenerateSqlAsync),
            description: "Complete the prompt.");

        var result = string.Empty;
        try
        {
            var invokeResult = await function.InvokeAsync(_kernel, arguments);
            result = invokeResult.GetValue<string>();

            arguments["execution_result"] = result;
        }
        catch(Exception exception)
        {
            _logger?.LogError("GenerateSqlAsync", exception);
            return $"Could not generate SQL request. Error: {exception.Message}";
        }

        return result;
    }

}