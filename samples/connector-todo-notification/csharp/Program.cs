using TeamsToDoAppConnector.Models.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AppSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.BaseUrl = configuration.GetValue<string>("BaseUrl");
    botOptions.ConnectorAppId = configuration.GetValue<string>("ConnectorAppId");
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

app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
