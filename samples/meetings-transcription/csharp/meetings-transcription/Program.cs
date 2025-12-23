using meetings_transcription;
using meetings_transcription.Controllers;
using meetings_transcription.Models.Configuration;
using meetings_transcription.Services;
using meetings_transcription.Helpers;
using meetings_transcription.Middleware;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AzureSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("Azure:MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("Azure:MicrosoftAppPassword");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("Azure:MicrosoftAppTenantId");
    botOptions.AppBaseUrl = configuration.GetValue<string>("Azure:AppBaseUrl");
    botOptions.UserId = configuration.GetValue<string>("Azure:UserId");
    botOptions.GraphApiEndpoint = configuration.GetValue<string>("Azure:GraphApiEndpoint");
});

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    // Configure Newtonsoft.Json to handle missing properties gracefully
    options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});

// Configure System.Text.Json for Teams SDK
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Creates Singleton Card Factory.
builder.Services.AddSingleton<ICardFactory, CardFactory>();

// Create a global hashset for our save task details
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

Func<string[], string?, Task<ITokenResponse>> createTokenFactory = async (string[] scopes, string? tenantId) =>
{
    var clientId = config.Teams.ClientId;

    var managedIdentityCredential = new ManagedIdentityCredential(clientId);
    var tokenRequestContext = new TokenRequestContext(scopes, tenantId: tenantId);
    var accessToken = await managedIdentityCredential.GetTokenAsync(tokenRequestContext);

    return new TokenResponse
    {
        TokenType = "Bearer",
        AccessToken = accessToken.Token,
    };
};
var appBuilder = App.Builder();

if (config.Teams.BotType == "UserAssignedMsi")
{
    appBuilder.AddCredentials(new TokenCredentials(
        config.Teams.ClientId ?? string.Empty,
        async (tenantId, scopes) =>
        {
            return await createTokenFactory(scopes, tenantId);
        }
    ));
}

builder.Services.AddSingleton<Controller>();
builder.AddTeams(appBuilder);
builder.Services.AddMvc().AddSessionStateTempDataProvider();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization();

// Intercept meeting events BEFORE Teams SDK processes them
app.UseMeetingEventInterceptor();

app.UseTeams();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();