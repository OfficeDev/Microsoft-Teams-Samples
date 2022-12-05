using GraphTeamsTag.Helper;
using GraphTeamsTag.Models.Configuration;
using GraphTeamsTag.Provider;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<SimpleBetaGraphClient>();

// Helpet
builder.Services.AddTransient<GraphHelper>();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AzureSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("AzureAd:ClientId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("AzureAd:AppSecret");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("AzureAd:TenantId");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();