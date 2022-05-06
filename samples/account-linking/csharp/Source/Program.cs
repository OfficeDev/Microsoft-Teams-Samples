using System.Net.Http.Headers;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Microsoft.Teams.Samples.AccountLinking;
using Microsoft.Teams.Samples.AccountLinking.GitHub;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.ReplayValidation;
using Microsoft.Teams.Samples.AccountLinking.UserTokenStorage;

using Microsoft.Teams.Samples.AccountLinking.Bots;
using Microsoft.Teams.Samples.AccountLinking.Dialogs;
using Microsoft.Teams.Samples.AccountLinking.State;

var builder = WebApplication.CreateBuilder(args);
var useAzure = builder.Configuration.GetValue<bool>("UseAzure");

var services = builder.Services;

services.AddOptions<TableStorageReplayValidatorOptions>()
    .BindConfiguration("StateReplay")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<TableStorageUserTokenStoreOptions>()
    .BindConfiguration("TokenStorage")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<OAuthOptions>()
    .BindConfiguration("OAuth")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<KeyringConfiguration>()
    .BindConfiguration("Keyring")
    .ValidateDataAnnotations()
    .ValidateOnStart();
services.AddOptions<AccountLinkingPromptOptions>()
    .BindConfiguration("AccountLinkingPrompt")
    .ValidateDataAnnotations()
    .ValidateOnStart();


if (useAzure)
{
    var azureCredential = new DefaultAzureCredential();

#pragma warning disable ASP0000 // No singletons declared above, just options objects
    var postConfigSp = services.BuildServiceProvider();
#pragma warning restore ASP0000

    // Configure the Microsoft.AspNetCore.DataProtection library for encrypting tokens
    // at rest and creating tamper-evident state values for OAuth flows
    var keyringOptions = postConfigSp.GetRequiredService<IOptions<KeyringConfiguration>>().Value;

    // Configure the DataProtection library to use blob storage for persistence & KeyVault for securely storing root key
    // We use this combination so that the bot can scale out w/o having incompatible encryption
    services
        .AddDataProtection()
        .PersistKeysToAzureBlobStorage(new Uri(keyringOptions.BlobUri), azureCredential)
        .ProtectKeysWithAzureKeyVault(new Uri(keyringOptions.KeyIdentifierUri), azureCredential);
    
    // Configure a 'replay validator' to ensure that user logins cannot be replayed. 
    // This uses Azure table storage to allow for scale-out of the bot.
    services.AddSingleton<IReplayValidator>(sp => {
        var cfg = sp.GetRequiredService<IOptions<TableStorageReplayValidatorOptions>>();
        var tableServiceClient = new TableServiceClient(
            endpoint: new Uri(cfg.Value.Endpoint),
            tokenCredential: azureCredential
        );

        return new TableStorageReplayValidator(
            logger: sp.GetRequiredService<ILogger<TableStorageReplayValidator>>(),
            tableServiceClient: tableServiceClient,
            options: cfg);
    });

    // Configure a database for the user token storage
    // Note this also sets up a decorator that transparently encrypts / decrypts the tokens so that they are encrypted at rest
    // This also uses Azure Table storage to enable scaled-out bots.
    services.AddSingleton<IUserTokenStore>(sp => {
        var cfg = sp.GetRequiredService<IOptions<TableStorageUserTokenStoreOptions>>();
        var tableClient = new TableClient(
            endpoint: new Uri(cfg.Value.Endpoint),
            tableName: cfg.Value.TableName,
            tokenCredential: azureCredential);

        var userTokenStorageImpl = new TableStorageUserTokenStore(
            logger: sp.GetRequiredService<ILogger<TableStorageUserTokenStore>>(),
            tableClient: tableClient);
            
        return new EncryptingUserTokenStoreDecorator(
            decorated: userTokenStorageImpl,
            dataProtectionProvider: sp.GetRequiredService<IDataProtectionProvider>(),
            logger: sp.GetRequiredService<ILogger<EncryptingUserTokenStoreDecorator>>());
    });
}
else
{
    // Configure the DataProtection library for in-memory / local
    services.AddDataProtection();

    // Configure the in-memory replay validator
    services.AddSingleton<IReplayValidator, InMemoryReplayValidator>();

    // Configure the in-memory token store
    services.AddSingleton<IUserTokenStore>(sp => {
        var userTokenStorageImpl = new InMemoryUserTokenStore(
            logger: sp.GetRequiredService<ILogger<InMemoryUserTokenStore>>()
        );

        return new EncryptingUserTokenStoreDecorator(
            decorated: userTokenStorageImpl,
            dataProtectionProvider: sp.GetRequiredService<IDataProtectionProvider>(),
            logger: sp.GetRequiredService<ILogger<EncryptingUserTokenStoreDecorator>>());
    });
}

services.AddTransient<AccountLinkingStateService>();
services.AddTransient<OAuthTokenProvider>();

// Add Microsoft.Identity.Web for the Github Controller so we can validate the Azure AD access token from the 
// Microsoft Teams tab
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Configure the Bot, dialog and botframework services 
services.AddSingleton<BotFrameworkAuthentication>(sp => {
    return new ConfigurationBotFrameworkAuthentication(builder.Configuration.GetSection("Bot"));
});

services.AddSingleton<IBotFrameworkHttpAdapter>(sp => {
    var adapter = new CloudAdapter(
        sp.GetRequiredService<BotFrameworkAuthentication>(),
        sp.GetRequiredService<ILogger<CloudAdapter>>());

    // Attach the middlewares to the adapter before injecting it.
    var middlewares = sp.GetRequiredService<IEnumerable<Microsoft.Bot.Builder.IMiddleware>>();
    foreach (var middleware in middlewares)
    {
        adapter.Use(middleware);
    }
    return adapter;
});

services.AddSingleton<IStorage, MemoryStorage>();
services.AddTransient<AccountLinkingPrompt>();
services.AddTransient<MainDialog>();
services.AddTransient<IBot, SampleActivityHandler<MainDialog>>();

services.AddSingleton<ConversationState>();
services.AddSingleton<UserState>();


// Ensure requests to GitHub have user-agent strings attached
// https://developer.github.com/v3/#user-agent-required
services.AddHttpClient<OAuthServiceClient>(cfg => {
    var productValue = new ProductInfoHeaderValue("GithubTeamsSSOintegrationSample", "1.0");
    cfg.DefaultRequestHeaders.UserAgent.Add(productValue);
});

services.AddHttpClient<GitHubServiceClient>(cfg => {
    var productValue = new ProductInfoHeaderValue("GithubTeamsSSOintegrationSample", "1.0");
    cfg.DefaultRequestHeaders.UserAgent.Add(productValue);
});


services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseDefaultFiles();
// Allow for static files to be hosted in the wwwroot directory so that the index.html can be used for the
// Microsoft Teams tab.
app.UseStaticFiles();

app.Run();
