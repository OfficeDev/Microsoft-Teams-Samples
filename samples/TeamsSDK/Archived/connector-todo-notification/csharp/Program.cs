using TeamsToDoAppConnector.Models.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Require an authenticated Microsoft Entra ID user before any endpoint runs.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Deny anonymous access globally; individual actions can opt out with [AllowAnonymous].
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddHttpClient();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AppSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.BaseUrl = configuration.GetValue<string>("BaseUrl");
    botOptions.ConnectorAppId = configuration.GetValue<string>("ConnectorAppId");
    botOptions.TenantId = configuration.GetValue<string>("TenantId");
    botOptions.AllowedWebhookHostSuffixes = configuration.GetSection("AllowedWebhookHostSuffixes").Get<string[]>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
