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

using Microsoft.Teams.Samples.AccountLinking.Service;
using Microsoft.Teams.Samples.AccountLinking.Service.OAuth;
using Microsoft.Teams.Samples.AccountLinking.Service.ReplayValidation;
using Microsoft.Teams.Samples.AccountLinking.Service.UserTokenStorage;

using Microsoft.Teams.Samples.AccountLinking.Service.State;

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
