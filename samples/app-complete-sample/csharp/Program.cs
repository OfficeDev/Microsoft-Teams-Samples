using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Teams.TemplateBotCSharp.Bots;
using Microsoft.Teams.TemplateBotCSharp.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Utility;
using template_bot_master_csharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the User state. (Used in this bot's Dialog implementation.)
builder.Services.AddSingleton<UserState>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

builder.Services.AddSingleton<PrivateConversationState>();

// Register the main dialog, which is injected into the DialogBot class
builder.Services.AddSingleton<RootDialog>();

// Register the DialogBot with RootDialog as the IBot interface
builder.Services.AddTransient<IBot, DialogBot<RootDialog>>();


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions<AzureSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.BotId = configuration.GetValue<string>("BotId");
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.BaseUri = configuration.GetValue<string>("BaseUri");
    botOptions.FBConnectionName = configuration.GetValue<string>("FBConnectionName");
    botOptions.FBProfileUrl = configuration.GetValue<string>("FBProfileUrl");
    botOptions.MaxComposeExtensionHistoryCount = configuration.GetValue<int>("MaxComposeExtensionHistoryCount");
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapDefaultControllerRoute();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    // Mapping of endpoints goes here:
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
       name: "default",
       pattern: "{controller=BotInfo}/{action=BotInfo}/{id?}");

});
app.Run();
