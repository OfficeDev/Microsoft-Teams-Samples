using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using SidePanel;
using SidePanel.Controllers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

if (config == null)
{
    throw new InvalidOperationException("Configuration is not properly set up.");
}

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

// Register HttpClientFactory for making HTTP requests
builder.Services.AddHttpClient();

// Register controllers
builder.Services.AddControllers();

// Register the Controller as a singleton
builder.Services.AddSingleton<Controller>();

// In production, the React files will be served from this directory
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});

// Add Teams SDK - this automatically registers App as a service
builder.AddTeams(appBuilder);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSpaStaticFiles();

// Enable routing and controllers
app.UseRouting();
app.MapControllers();

app.UseTeams();

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();