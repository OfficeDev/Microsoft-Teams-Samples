using TypeaheadSearch;
using TypeaheadSearch.Controllers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

// Add HttpClient factory
builder.Services.AddHttpClient();

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

var app = builder.Build();

// Add middleware to handle search invoke requests directly (bypassing SDK's broken deserialization)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.Path == "/api/messages" && 
        context.Request.ContentType?.Contains("application/json") == true)
    {
        context.Request.EnableBuffering();
        
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                var json = JsonDocument.Parse(body);
                var root = json.RootElement;

                // Check if this is an application/search invoke
                if (root.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "invoke" &&
                    root.TryGetProperty("name", out var nameElement) && nameElement.GetString() == "application/search")
                {
                    // Handle search invoke directly here to bypass SDK's broken deserialization
                    var controller = app.Services.GetRequiredService<Controller>();
                    var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                        .CreateLogger("SearchInvokeHandler");
                    
                    logger.LogInformation("Search invoke intercepted by middleware");
                    
                    // Extract the value object
                    if (root.TryGetProperty("value", out var valueElement))
                    {
                        var valueJson = valueElement.GetRawText();
                        var valueNode = System.Text.Json.Nodes.JsonNode.Parse(valueJson);
                        
                        var dataset = valueNode?["dataset"]?.ToString();
                        var queryText = valueNode?["queryText"]?.ToString() ?? string.Empty;
                        
                        logger.LogInformation($"Dataset: {dataset}, QueryText: {queryText}");
                        
                        object searchResponse = null;
                        
                        if (dataset == "npmpackages")
                        {
                            // Call NPM package search
                            var httpClientFactory = app.Services.GetRequiredService<System.Net.Http.IHttpClientFactory>();
                            var httpClient = httpClientFactory.CreateClient();
                            var packageResult = await httpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{queryText}&prerelease=true");
                            
                            var packageJson = System.Text.Json.Nodes.JsonNode.Parse(packageResult);
                            var data = packageJson?["data"]?.AsArray();
                            
                            var packages = new List<object>();
                            if (data != null)
                            {
                                foreach (var item in data)
                                {
                                    var id = item?["id"]?.ToString() ?? string.Empty;
                                    var description = item?["description"]?.ToString() ?? string.Empty;
                                    packages.Add(new { title = id, value = $"{id} - {description}" });
                                }
                            }
                            
                            searchResponse = new
                            {
                                type = "application/vnd.microsoft.search.searchResponse",
                                value = new { results = packages }
                            };
                            
                            logger.LogInformation($"Returning {packages.Count} NPM packages");
                        }
                        else if (dataset == "cities")
                        {
                            // Get country from associated inputs
                            var dataNode = valueNode?["data"];
                            string country = string.Empty;
                            
                            logger.LogInformation($"Data node: {dataNode?.ToJsonString()}");
                            
                            if (dataNode != null)
                            {
                                var countryValue = dataNode["choiceSelect"];
                                if (countryValue != null)
                                {
                                    country = countryValue.ToString().ToLower();
                                }
                            }
                            
                            logger.LogInformation($"Cities search for country: {country}");
                            
                            var usa = new[]
                            {
                                new { title = "CA", value = "CA" },
                                new { title = "FL", value = "FL" },
                                new { title = "TX", value = "TX" }
                            };

                            var france = new[]
                            {
                                new { title = "Paris", value = "Paris" },
                                new { title = "Lyon", value = "Lyon" },
                                new { title = "Nice", value = "Nice" }
                            };

                            var india = new[]
                            {
                                new { title = "Delhi", value = "Delhi" },
                                new { title = "Mumbai", value = "Mumbai" },
                                new { title = "Pune", value = "Pune" }
                            };

                            var results = country switch
                            {
                                "usa" => usa,
                                "france" => france,
                                _ => india
                            };
                            
                            searchResponse = new
                            {
                                type = "application/vnd.microsoft.search.searchResponse",
                                value = new { results }
                            };
                            
                            logger.LogInformation($"Returning {results.Length} cities for country: {country}");
                        }
                        
                        if (searchResponse != null)
                        {
                            // Return ONLY the searchResponse body directly (not wrapped in status/body)
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            
                            var responseJson = JsonSerializer.Serialize(searchResponse);
                            await context.Response.WriteAsync(responseJson);
                            
                            logger.LogInformation($"Sent response: {responseJson}");
                            return; // Don't call next() - we've handled this request
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                    .CreateLogger("SearchInvokeHandler");
                logger.LogError(ex, "Error in search invoke middleware");
                // Continue to next middleware on error
                context.Request.Body.Position = 0;
            }
        }
    }

    await next();
});

app.UseTeams();
app.Run();