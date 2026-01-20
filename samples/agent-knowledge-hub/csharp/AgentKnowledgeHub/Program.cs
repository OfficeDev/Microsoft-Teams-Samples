using AgentKnowledgeHub;
using AgentKnowledgeHub.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.Models;
using Microsoft.Teams.AI.AI.Planners;
using Microsoft.Teams.AI.AI.Prompts;
using Microsoft.Teams.AI.State;
using Microsoft.Teams.AI.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Prepare Configuration for ConfigurationBotFrameworkAuthentication
var config = builder.Configuration.Get<ConfigOptions>();
builder.Configuration["MicrosoftAppType"] = config.BOT_TYPE;
builder.Configuration["MicrosoftAppId"] = config.BOT_ID;
builder.Configuration["MicrosoftAppPassword"] = config.BOT_PASSWORD;
builder.Configuration["MicrosoftAppTenantId"] = config.BOT_TENANT_ID;
// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Cloud Adapter with error handling enabled.
// Note: some classes expect a BotAdapter and some expect a BotFrameworkHttpAdapter, so
// register the same adapter instance for both types.
builder.Services.AddSingleton<TeamsAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetService<TeamsAdapter>());
builder.Services.AddSingleton<BotAdapter>(sp => sp.GetService<TeamsAdapter>());

builder.Services.AddSingleton<IStorage, MemoryStorage>();

builder.Services.AddSingleton<OpenAIModel>(sp => new(
    new AzureOpenAIModelOptions(
        config.Azure.OpenAIApiKey,
        config.Azure.OpenAIDeploymentName,
        config.Azure.OpenAIEndpoint
    )
    {
        LogRequests = true,
    },
    sp.GetService<ILoggerFactory>()
));

// Create the bot as transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot>(sp =>
{
    // Create loggers
    ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>();

    // Create Prompt Manager
    PromptManager prompts = new(new()
    {
        PromptFolder = "./Prompts"
    });

    // Create ActionPlanner
    ActionPlanner<AppState> planner = new(
        options: new(
            model: sp.GetService<OpenAIModel>(),
            prompts: prompts,
            defaultPrompt: async (context, state, planner) =>
            {
                PromptTemplate template = prompts.GetPrompt("chat");
                return await Task.FromResult(template);
            }
        )
        { 
            LogRepairs = true,
        },
        loggerFactory: loggerFactory
    );

    AIOptions<AppState> options = new(planner);
    options.EnableFeedbackLoop = true;

    Application<AppState> app = new ApplicationBuilder<AppState>()
        .WithAIOptions(options)
        .WithStorage(sp.GetService<IStorage>())
        .Build();

    app.OnFeedbackLoop((turnContext, turnState, feedbackLoopData, _) =>
    {
        Console.WriteLine($"Your feedback is {turnContext.Activity.Value.ToString()}");
        return Task.CompletedTask;
    });

    return app;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();