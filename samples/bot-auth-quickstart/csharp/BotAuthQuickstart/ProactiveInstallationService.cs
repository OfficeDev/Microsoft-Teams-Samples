// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Teams.Api.Activities;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

namespace BotAuthQuickstart;

public static class ProactiveInstallationService
{
    private static string? _cachedCatalogAppId = null;

    public static GraphServiceClient CreateAppGraphClient(ConfigOptions? cfg)
    {
        if (string.IsNullOrEmpty(cfg?.Teams?.ClientId) || string.IsNullOrEmpty(cfg?.Teams?.ClientSecret) || string.IsNullOrEmpty(cfg?.Teams?.TenantId))
            throw new InvalidOperationException("Missing Graph credentials");
        return new GraphServiceClient(new ClientSecretCredential(cfg.Teams.TenantId, cfg.Teams.ClientId, cfg.Teams.ClientSecret), ["https://graph.microsoft.com/.default"]);
    }

    public static async Task<string?> GetCatalogAppId(GraphServiceClient graphClient, ConfigOptions? cfg, string? knownUserId = null)
    {
        if (!string.IsNullOrEmpty(_cachedCatalogAppId))
            return _cachedCatalogAppId;

        var clientId = cfg?.Teams?.ClientId;
        var teamsAppId = cfg?.Teams?.TeamsAppId;

        // Strategy 1: Try catalog lookup by externalId (needs AppCatalog.Read.All)
        if (!string.IsNullOrEmpty(clientId))
        {
            try
            {
                var catalogApps = await graphClient.AppCatalogs.TeamsApps.GetAsync(r =>
                {
                    r.QueryParameters.Filter = $"externalId eq '{clientId}'";
                });
                var found = catalogApps?.Value?.FirstOrDefault();
                if (found != null)
                {
                    _cachedCatalogAppId = found.Id;
                    return _cachedCatalogAppId;
                }
            }
            catch { }
        }

        // Strategy 2: Look up from a known user's installed apps
        if (!string.IsNullOrEmpty(knownUserId))
        {
            try
            {
                var installedApps = await graphClient.Users[knownUserId].Teamwork.InstalledApps.GetAsync(r =>
                {
                    r.QueryParameters.Expand = ["teamsApp"];
                });

                foreach (var appEntry in installedApps?.Value ?? [])
                {
                    var teamsApp = appEntry.TeamsApp;
                    var extId = teamsApp?.ExternalId;
                    if (!string.IsNullOrEmpty(extId) && (extId == clientId || extId == teamsAppId))
                    {
                        _cachedCatalogAppId = teamsApp!.Id;
                        return _cachedCatalogAppId;
                    }
                }
            }
            catch { }
        }

        // Strategy 3: Fall back to TeamsAppId from config
        if (!string.IsNullOrEmpty(teamsAppId))
            return teamsAppId;

        return null;
    }

    public static async Task<List<ConversationMember>> GetTeamMembers(GraphServiceClient graphClient, IActivity activity)
    {
        var conversation = activity.Conversation;
        var conversationId = conversation?.Id;

        if (string.IsNullOrEmpty(conversationId))
            return [];

        try
        {
            if (conversationId.Contains("@thread.tacv2") || conversationId.Contains("@thread.skype"))
            {
                var teamId = conversationId.Split(';')[0];
                var members = await graphClient.Teams[teamId].Members.GetAsync();
                return members?.Value ?? [];
            }
            else if (conversationId.StartsWith("19:") && !conversationId.Contains("@thread"))
            {
                var members = await graphClient.Chats[conversationId].Members.GetAsync();
                return members?.Value ?? [];
            }
            else
            {
                var members = await graphClient.Chats[conversationId].Members.GetAsync();
                return members?.Value ?? [];
            }
        }
        catch
        {
            return [];
        }
    }

    public static async Task<int> InstallAppForUser(GraphServiceClient graphClient, string targetUserId, string catalogAppId)
    {
        var installation = new UserScopeTeamsAppInstallation
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{catalogAppId}" }
            }
        };

        try
        {
            await graphClient.Users[targetUserId].Teamwork.InstalledApps.PostAsync(installation);
            return 201;
        }
        catch (Exception ex) when (ex.Message.Contains("Conflict") || ex.Message.Contains("already exists"))
        {
            return 409;
        }
        catch
        {
            return 500;
        }
    }

    public static async Task HandleInstallCommand(dynamic context, ConfigOptions? cfg)
    {
        var graphClient = CreateAppGraphClient(cfg);
        var activity = (IActivity)context.Activity;

        var knownUserId = activity.From?.AadObjectId;

        var catalogAppId = await GetCatalogAppId(graphClient, cfg, knownUserId);
        if (string.IsNullOrEmpty(catalogAppId))
        {
            await context.Send("Error: Could not determine the catalog app ID. Ensure the app is published to your organization's app catalog.");
            return;
        }

        await context.Send("Installing app for team members... Please wait.");

        var tenantId = activity.Conversation?.TenantId ?? cfg?.Teams?.TenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            await context.Send("Error: Could not determine tenant ID.");
            return;
        }

        var members = await GetTeamMembers(graphClient, activity);
        if (members.Count == 0)
        {
            await context.Send("Could not retrieve team members. Make sure this command is run in a team or group chat.");
            return;
        }

        int newInstalls = 0, existingInstalls = 0, errors = 0;

        foreach (var member in members)
        {
            var memberUserId = (member as AadUserConversationMember)?.UserId ?? member.Id;
            if (string.IsNullOrEmpty(memberUserId)) continue;

            try
            {
                var status = await InstallAppForUser(graphClient, memberUserId, catalogAppId);
                if (status == 201) newInstalls++;
                else if (status == 409) existingInstalls++;
                else errors++;
            }
            catch
            {
                errors++;
            }
        }

        await context.Send(
            $"**Installation Complete**\n\n" +
            $"- Newly Installed: {newInstalls}\n" +
            $"- Already Installed: {existingInstalls}\n" +
            $"- Errors: {errors}");
    }

    public static async Task HandleSendCommand(dynamic context, ConfigOptions? cfg)
    {
        var graphClient = CreateAppGraphClient(cfg);
        var activity = (IActivity)context.Activity;

        var knownUserId = activity.From?.AadObjectId;

        var catalogAppId = await GetCatalogAppId(graphClient, cfg, knownUserId);
        if (string.IsNullOrEmpty(catalogAppId))
        {
            await context.Send("Error: Could not determine the catalog app ID. Ensure the app is published to your organization's app catalog.");
            return;
        }

        await context.Send("Sending proactive notifications... Please wait.");

        var tenantId = activity.Conversation?.TenantId ?? cfg?.Teams?.TenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            await context.Send("Error: Could not determine tenant ID.");
            return;
        }

        var members = await GetTeamMembers(graphClient, activity);
        if (members.Count == 0)
        {
            await context.Send("Could not retrieve team members. Make sure this command is run in a team or group chat.");
            return;
        }

        await context.Send("Ensuring bot is installed for all members...");

        var memberUserIds = new List<string>();
        foreach (var member in members)
        {
            var memberUserId = (member as AadUserConversationMember)?.UserId ?? member.Id;
            if (string.IsNullOrEmpty(memberUserId)) continue;

            memberUserIds.Add(memberUserId);

            try
            {
                await InstallAppForUser(graphClient, memberUserId, catalogAppId);
            }
            catch { }
        }

        await Task.Delay(2000);

        int sentCount = 0, errorCount = 0;

        var botToken = await GetBotTokenAsync(cfg);
        if (string.IsNullOrEmpty(botToken))
        {
            await context.Send("Error: Could not obtain bot authentication token.");
            return;
        }

        foreach (var member in members)
        {
            var memberUserId = (member as AadUserConversationMember)?.UserId ?? member.Id;
            var displayName = member.DisplayName ?? "Unknown";
            if (string.IsNullOrEmpty(memberUserId)) continue;

            try
            {
                var installedApps = await graphClient.Users[memberUserId].Teamwork.InstalledApps.GetAsync(r =>
                {
                    r.QueryParameters.Expand = ["teamsApp"];
                    r.QueryParameters.Filter = $"teamsApp/id eq '{catalogAppId}'";
                });

                var installedApp = installedApps?.Value?.FirstOrDefault();
                if (installedApp == null)
                {
                    errorCount++;
                    continue;
                }

                var chat = await graphClient.Users[memberUserId].Teamwork.InstalledApps[installedApp.Id].Chat.GetAsync();
                if (chat == null || string.IsNullOrEmpty(chat.Id))
                {
                    errorCount++;
                    continue;
                }

                var success = await SendProactiveMessageViaBotConnector(
                    botToken,
                    cfg?.Teams?.ClientId ?? "",
                    chat.Id,
                    tenantId,
                    $"?? Proactive hello, {displayName}! This is a proactive message from the Auth Bot."
                );

                if (success) sentCount++;
                else errorCount++;
            }
            catch
            {
                errorCount++;
            }
        }

        await context.Send(
            $"**Notification Complete**\n\n" +
            $"- Messages Sent: {sentCount}\n" +
            $"- Errors: {errorCount}");
    }

    public static async Task<string?> GetBotTokenAsync(ConfigOptions? cfg)
    {
        if (string.IsNullOrEmpty(cfg?.Teams?.ClientId) || string.IsNullOrEmpty(cfg?.Teams?.ClientSecret) || string.IsNullOrEmpty(cfg?.Teams?.TenantId))
            return null;

        try
        {
            using var httpClient = new HttpClient();
            var tokenEndpoint = $"https://login.microsoftonline.com/{cfg.Teams.TenantId}/oauth2/v2.0/token";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = cfg.Teams.ClientId,
                ["client_secret"] = cfg.Teams.ClientSecret,
                ["scope"] = "https://api.botframework.com/.default"
            });

            var response = await httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> SendProactiveMessageViaBotConnector(string botToken, string botId, string conversationId, string tenantId, string message)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botToken);

            var serviceUrls = new[]
            {
                "https://smba.trafficmanager.net/teams/",
                "https://smba.trafficmanager.net/amer/",
                "https://smba.trafficmanager.net/emea/",
                "https://smba.trafficmanager.net/apac/"
            };

            foreach (var serviceUrl in serviceUrls)
            {
                var url = $"{serviceUrl}v3/conversations/{Uri.EscapeDataString(conversationId)}/activities";

                var activity = new
                {
                    type = "message",
                    text = message,
                    from = new { id = $"28:{botId}", name = "Auth Bot" },
                    conversation = new { id = conversationId, tenantId = tenantId },
                    channelData = new { tenant = new { id = tenantId } },
                    serviceUrl = serviceUrl
                };

                var json = JsonSerializer.Serialize(activity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode) return true;

                if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized &&
                    response.StatusCode != System.Net.HttpStatusCode.Forbidden)
                {
                    break;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static void MapProactiveEndpoints(WebApplication app, ConfigOptions? config, ConcurrentDictionary<string, ConversationReferenceData> conversationReferences)
    {
        // List all users
        app.MapGet("/api/users", async () =>
        {
            try
            {
                var graphClient = CreateAppGraphClient(config);
                var users = await graphClient.Users.GetAsync(r => { r.QueryParameters.Select = ["id", "displayName", "userPrincipalName"]; r.QueryParameters.Top = 999; });
                return Results.Ok(new { users = users?.Value?.Select(u => new { u.Id, u.DisplayName, u.UserPrincipalName }), count = users?.Value?.Count ?? 0 });
            }
            catch (Exception ex) { return Results.Problem($"Error: {ex.Message}"); }
        });

        // Get stored conversation references
        app.MapGet("/api/proactive/references", () => Results.Ok(new { count = conversationReferences.Count, references = conversationReferences.Select(kvp => new { userId = kvp.Key, conversationId = kvp.Value.ConversationId, tenantId = kvp.Value.TenantId }) }));

        // Install app for a specific user and send proactive message
        app.MapPost("/api/proactive/install/{userId}", async (string userId) =>
        {
            try
            {
                var graphClient = CreateAppGraphClient(config);
                var teamsAppId = config?.Teams?.TeamsAppId;

                if (string.IsNullOrEmpty(teamsAppId))
                    return Results.BadRequest(new { error = "TeamsAppId is not configured in appsettings.json" });

                var installation = new UserScopeTeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamsAppId}" }
                    }
                };

                try
                {
                    await graphClient.Users[userId].Teamwork.InstalledApps.PostAsync(installation);
                }
                catch (Exception ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("Conflict")) { }

                var installedApps = await graphClient.Users[userId].Teamwork.InstalledApps.GetAsync(r =>
                {
                    r.QueryParameters.Expand = ["teamsAppDefinition"];
                    r.QueryParameters.Filter = $"teamsApp/id eq '{teamsAppId}'";
                });

                var installedApp = installedApps?.Value?.FirstOrDefault();
                if (installedApp == null)
                    return Results.NotFound(new { error = "App installation not found for user" });

                var chat = await graphClient.Users[userId].Teamwork.InstalledApps[installedApp.Id].Chat.GetAsync();

                if (chat != null)
                {
                    var chatMessage = new ChatMessage
                    {
                        Body = new ItemBody
                        {
                            Content = "Hello! This is a proactive message from your Teams bot. The app has been installed successfully!"
                        }
                    };

                    await graphClient.Chats[chat.Id].Messages.PostAsync(chatMessage);
                    return Results.Ok(new { success = true, message = "App installed and proactive message sent", userId, chatId = chat.Id });
                }

                return Results.Ok(new { success = true, message = "App installed but could not send proactive message", userId });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error: {ex.Message}");
            }
        });

        // Install app for all users and send proactive messages
        app.MapPost("/api/proactive/install-all", async () =>
        {
            try
            {
                var graphClient = CreateAppGraphClient(config);
                var teamsAppId = config?.Teams?.TeamsAppId;

                if (string.IsNullOrEmpty(teamsAppId))
                    return Results.BadRequest(new { error = "TeamsAppId is not configured" });

                var users = await graphClient.Users.GetAsync(r =>
                {
                    r.QueryParameters.Select = ["id", "displayName", "userPrincipalName"];
                    r.QueryParameters.Top = 999;
                });

                var results = new List<object>();

                foreach (var user in users?.Value ?? [])
                {
                    try
                    {
                        var installation = new UserScopeTeamsAppInstallation
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamsAppId}" }
                            }
                        };

                        try { await graphClient.Users[user.Id].Teamwork.InstalledApps.PostAsync(installation); }
                        catch { }

                        var installedApps = await graphClient.Users[user.Id].Teamwork.InstalledApps.GetAsync(r =>
                        {
                            r.QueryParameters.Expand = ["teamsAppDefinition"];
                            r.QueryParameters.Filter = $"teamsApp/id eq '{teamsAppId}'";
                        });

                        var installedApp = installedApps?.Value?.FirstOrDefault();
                        if (installedApp != null)
                        {
                            var chat = await graphClient.Users[user.Id].Teamwork.InstalledApps[installedApp.Id].Chat.GetAsync();
                            if (chat != null)
                            {
                                var chatMessage = new ChatMessage
                                {
                                    Body = new ItemBody { Content = "?? Hello! This is a proactive message from your Teams bot." }
                                };
                                await graphClient.Chats[chat.Id].Messages.PostAsync(chatMessage);
                            }
                        }

                        results.Add(new { userId = user.Id, displayName = user.DisplayName, success = true });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { userId = user.Id, displayName = user.DisplayName, success = false, error = ex.Message });
                    }
                }

                return Results.Ok(new { message = "Proactive installation completed", totalUsers = users?.Value?.Count ?? 0, results });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error: {ex.Message}");
            }
        });

        // Send proactive notification to users who have interacted with the bot
        app.MapPost("/api/proactive/notify", async (HttpContext httpContext) =>
        {
            try
            {
                string message;
                try
                {
                    using var reader = new StreamReader(httpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var request = JsonSerializer.Deserialize<ProactiveNotifyRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    message = request?.Message ?? "Hello! This is a proactive notification from your Teams bot.";
                }
                catch
                {
                    message = "Hello! This is a proactive notification from your Teams bot.";
                }

                if (conversationReferences.IsEmpty)
                    return Results.Ok(new { success = false, message = "No conversation references found. Users need to interact with the bot first." });

                var graphClient = CreateAppGraphClient(config);
                var results = new List<object>();

                foreach (var kvp in conversationReferences)
                {
                    try
                    {
                        var reference = kvp.Value;
                        if (!string.IsNullOrEmpty(reference.ConversationId))
                        {
                            var chatMessage = new ChatMessage
                            {
                                Body = new ItemBody { Content = message }
                            };
                            await graphClient.Chats[reference.ConversationId].Messages.PostAsync(chatMessage);
                            results.Add(new { userId = kvp.Key, success = true });
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { userId = kvp.Key, success = false, error = ex.Message });
                    }
                }

                return Results.Ok(new { success = true, message = "Notifications sent", totalNotified = results.Count(r => ((dynamic)r).success), results });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error: {ex.Message}");
            }
        });
    }
}
