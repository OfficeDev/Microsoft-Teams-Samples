// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using static Microsoft.Teams.Api.Activities.Invokes.MessageExtensions;
using MsgExt = Microsoft.Teams.Api.MessageExtensions;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TeamsSettings>(builder.Configuration.GetSection("Teams"));
builder.AddTeams();

var app = builder.Build();

BotConfig.Settings = app.Services.GetRequiredService<IOptions<TeamsSettings>>().Value;

var teams = app.UseTeams();

// Create the handler instance
var handler = new BotMessageExtensionHandler();

// Message handler
teams.OnMessage(async (ctx) =>
{
    var text = ctx.Activity.Text?.ToLowerInvariant() ?? "";
    string replyText = text.Contains("help")
        ? "I'm the Search Messaging Extension Bot!\n\n" +
          "Use me in the compose area to search for NuGet packages or Wikipedia articles."
        : $"You said: {ctx.Activity.Text}\n\nType 'help' to learn about this bot.";

    await ctx.Send(replyText);
});

// Register message extension handlers using fluent API
teams.OnQuery(async (context) =>
{
    return await handler.OnQuery(context);
});

teams.OnSelectItem((context) =>
{
    return Task.FromResult(handler.OnSelectItem(context));
});

teams.OnQueryLink((context) =>
{
    return Task.FromResult(handler.OnQueryLink(context));
});

app.Run();

// --- Configuration ---

public static class BotConfig
{
    public static TeamsSettings Settings { get; set; } = new();
}

public class TeamsSettings
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string BaseUrl { get; set; } = "";
}

// --- Message Extension Handler ---

public class BotMessageExtensionHandler
{
    public async Task<MsgExt.Response> OnQuery(IContext<QueryActivity> ctx)
    {
        var query = ctx.Activity.Value;
        var commandId = query.CommandId;
        var parameters = query.Parameters;

        if (commandId == "wikipediaSearch")
        {
            var searchQuery = parameters?.FirstOrDefault(p => p.Name == "name")?.Value?.ToString() ?? "";
            ctx.Log.Info($"Wikipedia search: {searchQuery}");
            return await SearchWikipediaAsync(searchQuery, ctx);
        }
        else if (commandId == "searchByName")
        {
            var searchQuery = parameters?.FirstOrDefault(p => p.Name == "nameQuery")?.Value?.ToString() ?? "";
            ctx.Log.Info($"Expert search by name: {searchQuery}");
            return await SearchExpertsByNameAsync(searchQuery, ctx);
        }
        else if (commandId == "searchBySkill")
        {
            var searchQuery = parameters?.FirstOrDefault(p => p.Name == "skillQuery")?.Value?.ToString() ?? "";
            ctx.Log.Info($"Expert search by skill: {searchQuery}");
            return SearchExpertsBySkill(searchQuery, ctx);
        }
        else
        {
            var searchQuery = parameters?.FirstOrDefault(p => p.Name == "searchQuery")?.Value?.ToString() ?? "";
            ctx.Log.Info($"Search query: {searchQuery}");
            return await SearchNuGetPackagesAsync(searchQuery, ctx);
        }
    }

    public MsgExt.Response OnSelectItem(IContext<SelectItemActivity> ctx)
    {
        var rawValue = ctx.Activity.Value;
        ctx.Log.Info($"Item selected: {rawValue}");

        if (rawValue == null)
        {
            return CreateMessageResponse("No item selected");
        }

        var selectedItem = rawValue is JObject jo ? jo : JObject.FromObject(rawValue);

        var source = selectedItem["source"]?.ToString();
        if (source == "expertFinder")
        {
            return HandleExpertSelectItem(selectedItem, ctx);
        }

        var packageId = selectedItem["packageId"]?.ToString() ?? selectedItem["title"]?.ToString() ?? "Unknown";
        var version = selectedItem["version"]?.ToString() ?? "";
        var description = selectedItem["description"]?.ToString() ?? "";
        var projectUrl = selectedItem["projectUrl"]?.ToString() ?? "";

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            body = new object[]
            {
                new { type = "TextBlock", text = $"{packageId}, {version}", weight = "Bolder", size = "Large" },
                new { type = "TextBlock", text = description, wrap = true, isSubtle = true },
                new { type = "TextBlock", text = $"NuGet: https://www.nuget.org/packages/{packageId}", wrap = true, size = "Small" },
                new { type = "TextBlock", text = $"Project: {projectUrl}", wrap = true, size = "Small" }
            }
        };

        return new MsgExt.Response
        {
            ComposeExtension = new MsgExt.Result
            {
                Type = MsgExt.ResultType.Result,
                AttachmentLayout = Attachment.Layout.List,
                Attachments = new List<MsgExt.Attachment>
                {
                    new MsgExt.Attachment(ContentType.AdaptiveCard) { Content = card }
                }
            }
        };
    }

    public MsgExt.Response OnQueryLink(IContext<QueryLinkActivity> ctx)
    {
        var url = ctx.Activity.Value?.Url;
        ctx.Log.Info($"Query link: {url}");

        if (string.IsNullOrEmpty(url))
        {
            return CreateMessageResponse("No URL provided");
        }

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            body = new object[]
            {
                new { type = "TextBlock", text = "Link Preview", weight = "Bolder", size = "Medium" },
                new { type = "TextBlock", text = $"URL: {url}", isSubtle = true, wrap = true },
                new { type = "TextBlock", text = "This is a preview of the linked content generated by the message extension.", wrap = true, size = "Small" }
            }
        };

        var meAttachment = new MsgExt.Attachment(ContentType.AdaptiveCard)
        {
            Content = card,
            Preview = new Attachment(new ThumbnailCard
            {
                Title = "Link Preview",
                Text = url
            })
        };

        return new MsgExt.Response
        {
            ComposeExtension = new MsgExt.Result
            {
                Type = MsgExt.ResultType.Result,
                AttachmentLayout = Attachment.Layout.List,
                Attachments = new List<MsgExt.Attachment> { meAttachment }
            }
        };
    }

    // --- Search Methods ---

    async Task<MsgExt.Response> SearchWikipediaAsync(string query, IContext<QueryActivity> ctx)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BotMessageExtensions/1.0 (Teams Bot; +https://example.com)");
            var response = await httpClient.GetStringAsync(
                $"https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch={Uri.EscapeDataString(query)}&utf8=1");

            var json = JObject.Parse(response);
            var searchResults = json["query"]?["search"] as JArray;

            var attachments = new List<MsgExt.Attachment>();

            if (searchResults != null)
            {
                foreach (var result in searchResults.Take(8))
                {
                    var title = result["title"]?.ToString() ?? "No Title";
                    var snippet = result["snippet"]?.ToString() ?? "No snippet";

                    var card = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new object[]
                        {
                            new { type = "TextBlock", text = title, weight = "Bolder", size = "Large" },
                            new { type = "TextBlock", text = snippet, wrap = true, isSubtle = true }
                        }
                    };

                    attachments.Add(new MsgExt.Attachment(ContentType.AdaptiveCard)
                    {
                        Content = card,
                        Preview = new Attachment(new ThumbnailCard
                        {
                            Title = title,
                            Text = snippet
                        })
                    });
                }
            }

            return new MsgExt.Response
            {
                ComposeExtension = new MsgExt.Result
                {
                    Type = MsgExt.ResultType.Result,
                    AttachmentLayout = Attachment.Layout.List,
                    Attachments = attachments
                }
            };
        }
        catch (Exception ex)
        {
            ctx.Log.Error($"Wikipedia search error: {ex.Message}");
            return CreateMessageResponse("Failed to search Wikipedia");
        }
    }

    async Task<MsgExt.Response> SearchNuGetPackagesAsync(string query, IContext<QueryActivity> ctx)
    {
        try
        {
            using var httpClient = new HttpClient();
            var jsonString = await httpClient.GetStringAsync(
                $"https://azuresearch-usnc.nuget.org/query?q=id:{Uri.EscapeDataString(query)}&prerelease=true");

            var obj = JObject.Parse(jsonString);
            var data = obj["data"] as JArray;

            var attachments = new List<MsgExt.Attachment>();

            if (data != null)
            {
                foreach (var item in data)
                {
                    var packageId = item["id"]?.ToString() ?? "";
                    var description = item["description"]?.ToString() ?? "";

                    var card = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new object[]
                        {
                            new { type = "TextBlock", text = packageId, weight = "Bolder", size = "Large" }
                        }
                    };

                    attachments.Add(new MsgExt.Attachment(ContentType.AdaptiveCard)
                    {
                        Content = card,
                        Preview = new Attachment(new ThumbnailCard
                        {
                            Title = packageId,
                            Text = description
                        })
                    });
                }
            }

            return new MsgExt.Response
            {
                ComposeExtension = new MsgExt.Result
                {
                    Type = MsgExt.ResultType.Result,
                    AttachmentLayout = Attachment.Layout.List,
                    Attachments = attachments
                }
            };
        }
        catch (Exception ex)
        {
            ctx.Log.Error($"NuGet search error: {ex.Message}");
            return CreateMessageResponse("Failed to search NuGet packages");
        }
    }

    // --- Expert Finder Methods ---

    async Task<string> GetGraphAccessTokenAsync(Microsoft.Teams.Common.Logging.ILogger log)
    {
        var settings = BotConfig.Settings;
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(
            $"https://login.microsoftonline.com/{settings.TenantId}/oauth2/v2.0/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["grant_type"] = "client_credentials"
            }));

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            log.Error($"Failed to get Graph token: {responseBody}");
            throw new Exception("Failed to acquire Microsoft Graph access token.");
        }

        return JObject.Parse(responseBody)["access_token"]?.ToString()
            ?? throw new Exception("No access token in response.");
    }

    async Task<MsgExt.Response> SearchExpertsByNameAsync(string query, IContext<QueryActivity> ctx)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return CreateMessageResponse("Please enter a name to search for experts in your organization.");
        }

        try
        {
            var accessToken = await GetGraphAccessTokenAsync(ctx.Log);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

            var graphUrl = $"https://graph.microsoft.com/v1.0/users?$search=\"displayName:{query}\"&$select=id,displayName,jobTitle,department,mail,officeLocation,businessPhones,userPrincipalName&$top=10&$count=true";
            ctx.Log.Info($"Graph API call: {graphUrl}");

            var response = await httpClient.GetAsync(graphUrl);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ctx.Log.Error($"Graph API error: {response.StatusCode} - {responseBody}");
                return CreateMessageResponse("Failed to search users. Make sure User.Read.All permission is granted.");
            }

            var json = JObject.Parse(responseBody);
            var users = json["value"] as JArray;

            if (users == null || users.Count == 0)
            {
                return CreateMessageResponse($"No users found matching '{query}'.");
            }

            var attachments = new List<MsgExt.Attachment>();
            foreach (var user in users)
            {
                var displayName = user["displayName"]?.ToString() ?? "Unknown";
                var jobTitle = user["jobTitle"]?.ToString() ?? "N/A";
                var department = user["department"]?.ToString() ?? "N/A";
                var email = user["mail"]?.ToString() ?? "N/A";
                var officeLocation = user["officeLocation"]?.ToString() ?? "N/A";
                var phones = user["businessPhones"] as JArray;
                var phone = phones?.FirstOrDefault()?.ToString() ?? "N/A";
                var userPrincipalName = user["userPrincipalName"]?.ToString() ?? "";

                attachments.Add(BuildExpertAttachment(displayName, jobTitle, department, email, officeLocation, phone, userPrincipalName, null));
            }

            return new MsgExt.Response
            {
                ComposeExtension = new MsgExt.Result
                {
                    Type = MsgExt.ResultType.Result,
                    AttachmentLayout = Attachment.Layout.List,
                    Attachments = attachments
                }
            };
        }
        catch (Exception ex)
        {
            ctx.Log.Error($"Expert search by name error: {ex.Message}");
            return CreateMessageResponse($"Failed to search experts: {ex.Message}");
        }
    }

    MsgExt.Response SearchExpertsBySkill(string query, IContext<QueryActivity> ctx)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return CreateMessageResponse("Please enter a skill or topic to find experts.");
        }

        try
        {
            var expertsFilePath = Path.Combine(AppContext.BaseDirectory, "experts.json");
            if (!File.Exists(expertsFilePath))
            {
                expertsFilePath = Path.Combine(".", "experts.json");
            }

            var expertsJson = File.ReadAllText(expertsFilePath);
            var experts = JArray.Parse(expertsJson);

            var queryLower = query.ToLowerInvariant();
            var matchedExperts = experts.Where(e =>
            {
                var skills = e["skills"] as JArray;
                var jobTitle = e["jobTitle"]?.ToString()?.ToLowerInvariant() ?? "";
                var department = e["department"]?.ToString()?.ToLowerInvariant() ?? "";
                var about = e["about"]?.ToString()?.ToLowerInvariant() ?? "";

                bool skillMatch = skills != null && skills.Any(s => s.ToString().ToLowerInvariant().Contains(queryLower));
                bool titleMatch = jobTitle.Contains(queryLower);
                bool deptMatch = department.Contains(queryLower);
                bool aboutMatch = about.Contains(queryLower);

                return skillMatch || titleMatch || deptMatch || aboutMatch;
            }).Take(10).ToList();

            if (matchedExperts.Count == 0)
            {
                return CreateMessageResponse($"No experts found with skill or topic '{query}'. Try: C#, Azure, React, Python, Security, DevOps, etc.");
            }

            var attachments = new List<MsgExt.Attachment>();
            foreach (var expert in matchedExperts)
            {
                var displayName = expert["displayName"]?.ToString() ?? "Unknown";
                var jobTitle = expert["jobTitle"]?.ToString() ?? "N/A";
                var department = expert["department"]?.ToString() ?? "N/A";
                var email = expert["email"]?.ToString() ?? "N/A";
                var officeLocation = expert["officeLocation"]?.ToString() ?? "N/A";
                var phone = expert["phone"]?.ToString() ?? "N/A";
                var skills = expert["skills"] as JArray;
                var skillsList = skills != null ? string.Join(", ", skills.Select(s => s.ToString())) : "N/A";
                var about = expert["about"]?.ToString();

                attachments.Add(BuildExpertAttachment(displayName, jobTitle, department, email, officeLocation, phone, email, skillsList, about));
            }

            return new MsgExt.Response
            {
                ComposeExtension = new MsgExt.Result
                {
                    Type = MsgExt.ResultType.Result,
                    AttachmentLayout = Attachment.Layout.List,
                    Attachments = attachments
                }
            };
        }
        catch (Exception ex)
        {
            ctx.Log.Error($"Expert search by skill error: {ex.Message}");
            return CreateMessageResponse($"Failed to search experts: {ex.Message}");
        }
    }

    MsgExt.Response HandleExpertSelectItem(JObject selectedItem, IContext<SelectItemActivity> ctx)
    {
        var displayName = selectedItem["displayName"]?.ToString() ?? "Unknown";
        var jobTitle = selectedItem["jobTitle"]?.ToString() ?? "N/A";
        var department = selectedItem["department"]?.ToString() ?? "N/A";
        var email = selectedItem["email"]?.ToString() ?? "N/A";
        var officeLocation = selectedItem["officeLocation"]?.ToString() ?? "N/A";
        var phone = selectedItem["phone"]?.ToString() ?? "N/A";
        var userPrincipalName = selectedItem["userPrincipalName"]?.ToString() ?? email;
        var skills = selectedItem["skills"]?.ToString() ?? "N/A";
        var about = selectedItem["about"]?.ToString() ?? "";

        ctx.Log.Info($"Expert selected: {displayName}");

        var bodyItems = new List<object>
        {
            new
            {
                type = "ColumnSet",
                columns = new object[]
                {
                    new
                    {
                        type = "Column",
                        width = "auto",
                        items = new object[]
                        {
                            new { type = "Image", url = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png", size = "Large", style = "Person" }
                        }
                    },
                    new
                    {
                        type = "Column",
                        width = "stretch",
                        items = new object[]
                        {
                            new { type = "TextBlock", text = displayName, weight = "Bolder", size = "Large" },
                            new { type = "TextBlock", text = jobTitle, isSubtle = true, spacing = "None" },
                            new { type = "TextBlock", text = department, isSubtle = true, spacing = "None", size = "Small" }
                        }
                    }
                }
            },
            new
            {
                type = "FactSet",
                facts = new object[]
                {
                    new { title = "Email", value = email },
                    new { title = "Office", value = officeLocation },
                    new { title = "Phone", value = phone },
                    new { title = "Skills", value = skills }
                }
            }
        };

        if (!string.IsNullOrEmpty(about))
        {
            bodyItems.Add(new { type = "TextBlock", text = "**About**", weight = "Bolder", spacing = "Medium" });
            bodyItems.Add(new { type = "TextBlock", text = about, wrap = true, isSubtle = true });
        }

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            body = bodyItems.ToArray(),
            actions = new object[]
            {
                new { type = "Action.OpenUrl", title = "Chat in Teams", url = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userPrincipalName)}" },
                new { type = "Action.OpenUrl", title = "Send Email", url = $"mailto:{email}" }
            }
        };

        return new MsgExt.Response
        {
            ComposeExtension = new MsgExt.Result
            {
                Type = MsgExt.ResultType.Result,
                AttachmentLayout = Attachment.Layout.List,
                Attachments = new List<MsgExt.Attachment>
                {
                    new MsgExt.Attachment(ContentType.AdaptiveCard) { Content = card }
                }
            }
        };
    }

    MsgExt.Attachment BuildExpertAttachment(string displayName, string jobTitle, string department, string email, string officeLocation, string phone, string userPrincipalName, string? skills, string? about = null)
    {
        var bodyItems = new List<object>
        {
            new
            {
                type = "ColumnSet",
                columns = new object[]
                {
                    new
                    {
                        type = "Column",
                        width = "auto",
                        items = new object[]
                        {
                            new { type = "Image", url = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png", size = "Medium", style = "Person" }
                        }
                    },
                    new
                    {
                        type = "Column",
                        width = "stretch",
                        items = new object[]
                        {
                            new { type = "TextBlock", text = displayName, weight = "Bolder", size = "Medium" },
                            new { type = "TextBlock", text = jobTitle, isSubtle = true, spacing = "None", size = "Small" },
                            new { type = "TextBlock", text = department, isSubtle = true, spacing = "None", size = "Small" }
                        }
                    }
                }
            },
            new
            {
                type = "FactSet",
                facts = new object[]
                {
                    new { title = "Email", value = email },
                    new { title = "Office", value = officeLocation },
                    new { title = "Phone", value = phone }
                }
            }
        };

        if (!string.IsNullOrEmpty(skills))
        {
            bodyItems.Add(new { type = "TextBlock", text = $"**Skills:** {skills}", wrap = true, size = "Small" });
        }

        var card = new
        {
            type = "AdaptiveCard",
            version = "1.4",
            body = bodyItems.ToArray(),
            actions = new object[]
            {
                new { type = "Action.OpenUrl", title = "Chat", url = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userPrincipalName)}" },
                new { type = "Action.OpenUrl", title = "Email", url = $"mailto:{email}" }
            }
        };

        var previewText = !string.IsNullOrEmpty(skills) ? $"{jobTitle} | {skills}" : $"{jobTitle} | {department}";

        return new MsgExt.Attachment(ContentType.AdaptiveCard)
        {
            Content = card,
            Preview = new Attachment(new HeroCard
            {
                Title = displayName,
                Text = previewText,
                Images = new List<Microsoft.Teams.Api.Cards.Image>
                {
                    new() { Url = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png" }
                }
            })
        };
    }

    // --- Helper Methods ---

    static MsgExt.Response CreateMessageResponse(string message)
    {
        return new MsgExt.Response
        {
            ComposeExtension = new MsgExt.Result
            {
                Type = MsgExt.ResultType.Message,
                Text = message
            }
        };
    }
}
