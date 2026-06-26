using ClientSideConfig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using TeamsTalentMgmtApp;
using TeamsTalentMgmtApp.Bot;
using TeamsTalentMgmtApp.Bot.Dialogs;
using TeamsTalentMgmtApp.Controllers;
using TeamsTalentMgmtApp.Infrastructure;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Services.Data;
using TeamTalentMgmtApp.AutoMapper;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<AppSettings>(builder.Configuration);
var appSettings = builder.Configuration.Get<AppSettings>();

// Services

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = $"https://login.microsoftonline.com/{appSettings.MicrosoftDirectoryId}/v2.0";
    options.Audience = appSettings.MicrosoftAppId;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerValidator = MultitenantWildcardIssuerValidator,
        NameClaimType = "name",
        SignatureValidator = (token, s) => new JwtSecurityToken(token)
    };
});

builder.Services.AddSingleton(bc => NullBotTelemetryClient.Instance);

// Configure credentials
builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
builder.Services.AddSingleton(new MicrosoftAppCredentials(appSettings.MicrosoftAppId, appSettings.MicrosoftAppPassword));

// Configure adapters
builder.Services.AddSingleton<BotLoggingMiddleware>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, TeamsBotHttpAdapter>(x =>
{
    var adapter = new TeamsBotHttpAdapter(
        x.GetRequiredService<IWebHostEnvironment>(),
        x.GetRequiredService<IConfiguration>(),
        x.GetRequiredService<ILogger<TeamsBotHttpAdapter>>(),
        x.GetRequiredService<IBotTelemetryClient>(),
        x.GetService<ConversationState>()
        );

    adapter.Use(x.GetService<BotLoggingMiddleware>());

    return adapter;
});

// Configure storage
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddDbContext<DatabaseContext>();

// Dialogs
builder.Services.AddTransient<CandidateDetailsDialog>();
builder.Services.AddTransient<CandidateSummaryDialog>();
builder.Services.AddTransient<HelpDialog>();
builder.Services.AddTransient<MainDialog>();
builder.Services.AddTransient<NewJobPostingDialog>();
builder.Services.AddTransient<OpenPositionsDialog>();
builder.Services.AddTransient<PositionsDetailsDialog>();
builder.Services.AddTransient<SignOutDialog>();
builder.Services.AddTransient<TopCandidatesDialog>();
builder.Services.AddTransient<NewTeamDialog>();
builder.Services.AddTransient<InstallBotDialog>();

// Templates
builder.Services.AddTransient<CandidatesTemplate>();
builder.Services.AddTransient<PositionsTemplate>();
builder.Services.AddTransient<NewJobPostingToAdaptiveCardTemplate>();

// Services
builder.Services.AddTransient<IRecruiterService, RecruiterService>();
builder.Services.AddTransient<ICandidateService, CandidateService>();
builder.Services.AddTransient<IPositionService, PositionService>();
builder.Services.AddTransient<ILocationService, LocationService>();
builder.Services.AddTransient<IInterviewService, InterviewService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IBotService, BotService>();
builder.Services.AddTransient<IGraphApiService, GraphApiService>();
builder.Services.AddTransient<IInvokeActivityHandler, InvokeActivityHandler>();

builder.Services.AddHttpClient();

builder.Services.AddTransient<IBot, TeamsTalentMgmtBot>();
builder.Services
    .AddMvc(options => { options.EnableEndpointRouting = false; })
    .AddJsonOptions(options => { options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; });

builder.Services.AddAutoMapper(typeof(TeamsTalentAppBaseProfile), typeof(TeamsTalentMgmtProfile));

// Configure

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseClientSideConfig(
    new
    {
        microsoftAppId = appSettings.MicrosoftAppId,
        applicationIdUri = appSettings.ApplicationIdUri
    }, new ClientSideConfigJsOptions());

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticViews")),
    RequestPath = "/StaticViews"
});

app.UseCors("CorsPolicy");
app.UseStatusCodePages();

app.UseAuthorization();
app.UseAuthentication();

app.UseMvc();

using var serviceScope = app.Services.CreateScope();

var scopeServiceProvider = serviceScope.ServiceProvider;
var db = scopeServiceProvider.GetService<DatabaseContext>();
var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
var dataSeedPath = Path.Combine(currentDirectory, "SampleData");
SampleData.InitializeDatabase(dataSeedPath, db);

app.Run();

static string MultitenantWildcardIssuerValidator(string issuer, SecurityToken token, TokenValidationParameters parameters)
{
    if (token is JwtSecurityToken jwt)
    {
        if (jwt.Issuer == "https://api.botframework.com")
        {
            return jwt.Issuer;
        }

        var tokenTenantId = jwt.Claims.Where(c => c.Type == "tid").FirstOrDefault().Value;
        if (issuer == $"https://login.microsoftonline.com/{tokenTenantId}/v2.0")
        {
            return issuer;
        }
    }

    // Recreate the exception that is thrown by default
    // when issuer validation fails
    var validIssuer = parameters.ValidIssuer ?? "null";
    var validIssuers = parameters.ValidIssuers == null
        ? "null"
        : !parameters.ValidIssuers.Any()
            ? "empty"
            : string.Join(", ", parameters.ValidIssuers);
    string errorMessage = FormattableString.Invariant(
        $"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match: validationParameters.ValidIssuer: '{validIssuer}' or validationParameters.ValidIssuers: '{validIssuers}'.");

    throw new SecurityTokenInvalidIssuerException(errorMessage)
    {
        InvalidIssuer = issuer
    };
}
