using EventMeeting;
using EventMeeting.Helper;
using GraphTeamsTag.Models.Configuration;
using EventMeeting.Provider;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<SimpleBetaGraphClient>();

// Helpet
builder.Services.AddTransient<GraphHelper>();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AzureSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("MicrosoftAppTenantId");
});

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting() 
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.UseSpaStaticFiles();
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();
