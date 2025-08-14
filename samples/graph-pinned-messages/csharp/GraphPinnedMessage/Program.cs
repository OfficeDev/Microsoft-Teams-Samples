using GraphPinnedMessage.Models.Configuration;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllers();

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
app.UseStaticFiles();

app.UseSpaStaticFiles();
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