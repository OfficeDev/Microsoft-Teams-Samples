// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Localization.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Samples.HelloWorld.Web;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add localization services and set the resources path.
builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

// Configure request localization options (supported cultures, fallback logic, etc.)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("fr-CA"),
        new CultureInfo("hi-IN"),
        new CultureInfo("es-MX")
    };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.FallBackToParentCultures = false;
});

// Add MVC services for controller handling.
builder.Services.AddControllers();

// Configure MVC to use Razor views and add localization support.
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
    .AddDataAnnotationsLocalization();

// Register BotFrameworkAuthentication and CloudAdapter with error handling.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Register the bot as a transient service.
builder.Services.AddTransient<IBot, LocalizerBot>();

var app = builder.Build();

// Enable detailed error pages in development environment.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

// Serve static files and default files.
app.UseDefaultFiles();
app.UseStaticFiles();

// Enable WebSockets.
app.UseWebSockets();

// Apply localization middleware.
var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions!.Value);

// Configure routing and endpoint mapping.
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
