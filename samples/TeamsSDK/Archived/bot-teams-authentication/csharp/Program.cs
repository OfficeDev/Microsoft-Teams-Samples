// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state, as well as Single Sign On.
// (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// For SSO, use CosmosDbPartitionedStorage

/* COSMOSDB STORAGE - Uncomment the code in this section to use CosmosDB storage */

// var cosmosDbStorageOptions = new CosmosDbPartitionedStorageOptions()
// {
//     CosmosDbEndpoint = "<endpoint-for-your-cosmosdb-instance>",
//     AuthKey = "<your-cosmosdb-auth-key>",
//     DatabaseId = "<your-database-id>",
//     ContainerId = "<cosmosdb-container-id>"
// };
// var storage = new CosmosDbPartitionedStorage(cosmosDbStorageOptions);

/* END COSMOSDB STORAGE */

// Create the User state. (Used in this bot's Dialog implementation.)
builder.Services.AddSingleton<UserState>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, TeamsBot<MainDialog>>();

// Add logging
builder.Logging.AddDebug();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

// app.UseHttpsRedirection();

app.Run();
