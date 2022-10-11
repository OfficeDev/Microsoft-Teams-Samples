// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Teams.Apps.SelfHelp.AdaptiveCard.Services;
using Microsoft.Teams.Apps.Selfhelp.Helper;
using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.ArticleRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.FeedbackRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LearningPath;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LogEventRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserReactionRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;
using Microsoft.Teams.Apps.Selfhelp.Shared.Services.BingSearch;
using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper;
using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token;
using Microsoft.Teams.Selfhelp.Authentication;
using Microsoft.Teams.Selfhelp.Authentication.Bot;
using Microsoft.Teams.Selfhelp.Bot;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add builder.Services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
builder.Services.AddOptions();
builder.Services.AddLocalization();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TelemetryClient>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddTransient(serviceProvider => (BotFrameworkAdapter)serviceProvider.GetRequiredService<IBotFrameworkHttpAdapter>());
builder.Services.AddSingleton<BotFrameworkHttpAdapter>();
builder.Services.AddTransient<IBot, SelfHelpActivityHandler>();
builder.Services.AddTransient<SelfHelpActivityMiddleware>();

// Creating the storage.
var storage = new MemoryStorage();

// Create the Conversation state passing in the storage layer.
var conversationState = new ConversationState(storage);
builder.Services.AddSingleton(conversationState);

// Register credential provider
ICredentialProvider credentialProvider = new SimpleCredentialProvider(
                appId: builder.Configuration["MicrosoftAppId"],
                password: builder.Configuration["MicrosoftAppPassword"]);

builder.Services.AddSingleton(credentialProvider);

// Register confidential client app
IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(builder.Configuration["MicrosoftAppId"])
    .WithClientSecret(builder.Configuration["MicrosoftAppPassword"])
    .Build();

confidentialClientApp.AddInMemoryTokenCache();

builder.Services.AddSingleton<IConfidentialClientApplication>(confidentialClientApp);

builder.Services.RegisterAuthenticationServices(builder.Configuration);

builder.Services.AddSingleton(new MicrosoftAppCredentials(builder.Configuration["MicrosoftAppId"], builder.Configuration["MicrosoftAppPassword"]));

// Add Microsoft Graph Services.

// Add repositories.
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IArticleRepository, ArticleRepository>();
builder.Services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddSingleton<ILearningPathRepository, LearningPathRepository>();
builder.Services.AddSingleton<IUserReactionRepository, UserReactionRepository>();
builder.Services.AddSingleton<IAppConfigRepository, AppConfigRepository>();
builder.Services.AddSingleton<ILogEventRepository, LogEventRepository>();
builder.Services.AddSingleton<ITeamRepository, TeamRepository>();

// Add services
builder.Services.AddSingleton<IAdaptiveCardService, AdaptiveCardService>();
builder.Services.AddSingleton<IBingSearch, BingSearch>();
builder.Services.AddSingleton<IGraphApiHelper, GraphApiHelper>();
builder.Services.AddSingleton<ITokenHelper, TokenHelper>();
builder.Services.AddSingleton<IMessageService, MessageService>();

// Add all options set from configuration values.
builder.Services.AddOptions<BlobStorageSetting>()
.Configure<IConfiguration>((storageSetting, configuration) =>
{
    // NOTE: This AzureAd:Instance configuration setting does not need to be
    // overridden by any deployment specific value. It can stay the default value
    // that is set in the project's configuration.
    storageSetting.ConnectionString = configuration.GetValue<string>("StorageConnectionString");
});

builder.Services.AddOptions<BotSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
    botOptions.AppBaseUri = configuration.GetValue<string>("App:AppBaseUri");
    botOptions.ProfileImageCacheDurationInMinutes = configuration.GetValue<double>("App:ProfileImageCacheDurationInMinutes");
    botOptions.UPN = configuration.GetValue<string>("AdminSettings:UPN");
});

builder.Services.AddOptions<BingSearchSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.BingSearchEndpoint = configuration.GetValue<string>("BingSearch:BingSearchEndpoint");
    botOptions.BingSearchSubscriptionKey = configuration.GetValue<string>("BingSearch:BingSearchSubscriptionKey");
});

builder.Services.AddOptions<BotFilterMiddlewareOptions>()
    .Configure<IConfiguration>((botFilterMiddlewareOptions, configuration) =>
    {
        botFilterMiddlewareOptions.DisableTenantFilter = configuration.GetValue<bool>("DisableTenantFilter");
        botFilterMiddlewareOptions.AllowedTenants = configuration.GetValue<string>("MicrosoftAppPassword");
    });

// In production, the React files will be served from this directory
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});

var app = builder.Build();

// Configure the HTTP request pipeline.l
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseRequestLocalization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSpaStaticFiles();
app.UseMvc();
app.UseRouting();

app.MapControllerRoute(
name: "default",
pattern: "{controller}/{action=Index}/{id?}");

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();