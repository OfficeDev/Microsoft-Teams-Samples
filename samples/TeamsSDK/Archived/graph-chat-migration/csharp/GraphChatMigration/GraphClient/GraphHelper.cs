// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Agents.Builder;

namespace GraphChatMigration.GraphClient
{
    public class GraphHelper
    {
        private readonly IConfiguration _configuration;
        private string _accessToken;
        private readonly string _userId;
        private readonly ILogger<GraphHelper> _logger;

        public GraphHelper(IConfiguration configuration, ILogger<GraphHelper> logger = null)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Get user ID from configuration
            _userId = configuration["UserId"] ?? "default-user-id";
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
                return _accessToken;

            try
            {
                string clientId = _configuration["Connections:BotServiceConnection:Settings:ClientId"];
                string clientSecret = _configuration["Connections:BotServiceConnection:Settings:ClientSecret"];
                string tenantId = _configuration["Connections:BotServiceConnection:Settings:TenantId"];

                // Log the configuration values being used (without the secret)
                _logger?.LogInformation($"Getting access token with ClientId: {clientId}, TenantId: {tenantId}");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId))
                {
                    _logger?.LogWarning("ClientId or TenantId is missing from configuration");
                }

                _accessToken = await SimpleGraphClient.GetAccessToken(clientId, clientSecret, tenantId);
                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting access token");
                throw;
            }
        }

        // Detects the conversation type and extracts relevant IDs
        public ConversationInfo GetConversationInfo(string conversationId, object channelData)
        {
            // Remove message ID if present in the conversation ID
            string cleanConversationId = conversationId;
            if (conversationId != null && conversationId.Contains(";messageid="))
            {
                cleanConversationId = conversationId.Split(";messageid=")[0];
                _logger?.LogInformation($"Removed message ID from conversation ID: {conversationId} -> {cleanConversationId}");
            }

            // Check if it's a Teams channel (contains @thread.tacv2)
            if (cleanConversationId != null && cleanConversationId.Contains("@thread.tacv2"))
            {
                string teamId = null;

                // Try to extract team ID from channelData
                if (channelData is JsonElement jsonElement && 
                    jsonElement.TryGetProperty("team", out JsonElement teamElement) && 
                    teamElement.TryGetProperty("id", out JsonElement idElement))
                {
                    teamId = idElement.GetString();
                }

                return new ConversationInfo
                {
                    Type = "channel",
                    TeamId = teamId,
                    ChannelId = cleanConversationId,
                    ConversationId = cleanConversationId
                };
            }
            // Check if it's a group chat (starts with 19: but no @thread.tacv2)
            else if (cleanConversationId != null && cleanConversationId.StartsWith("19:"))
            {
                return new ConversationInfo
                {
                    Type = "groupchat",
                    ChatId = cleanConversationId,
                    ConversationId = cleanConversationId
                };
            }
            // Default fallback
            else
            {
                return new ConversationInfo
                {
                    Type = "unknown",
                    ConversationId = cleanConversationId
                };
            }
        }

        // Get team ID from turn context using channel data (for Agents SDK)
        public async Task<string> ExtractTeamIdFromTurnContextAsync(ITurnContext turnContext)
        {
            try
            {
                if (turnContext == null)
                {
                    _logger?.LogWarning("TurnContext is null in ExtractTeamIdFromTurnContextAsync");
                    return null;
                }

                // Try to extract team ID from channel data
                if (turnContext.Activity.ChannelData is JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty("team", out JsonElement teamElement) &&
                        teamElement.TryGetProperty("id", out JsonElement idElement))
                    {
                        string teamId = idElement.GetString();
                        _logger?.LogInformation($"Extracted team ID from channel data: {teamId}");
                        return teamId;
                    }
                }
                
                _logger?.LogWarning("Could not extract team ID from turn context");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting team ID from turn context");
                return null;
            }
        }

        public async Task<HttpResponseMessage> StartMigrationAsync(ConversationInfo conversationInfo)
        {
            var accessToken = await GetAccessTokenAsync();
            string id = GetAppropriateId(conversationInfo);

            return await SimpleGraphClient.StartMigration(accessToken, conversationInfo.Type, id);
        }

        public async Task<HttpResponseMessage> CompleteMigrationAsync(ConversationInfo conversationInfo)
        {
            var accessToken = await GetAccessTokenAsync();
            string id = GetAppropriateId(conversationInfo);

            return await SimpleGraphClient.CompleteMigration(accessToken, conversationInfo.Type, id);
        }

        public async Task<HttpResponseMessage> PostMessageWithTimestampAsync(ConversationInfo conversationInfo, string messageContent, DateTime timestamp)
        {
            var accessToken = await GetAccessTokenAsync();
            string id = GetAppropriateId(conversationInfo);

            // Convert DateTime to DateTimeOffset for Graph API compatibility
            DateTimeOffset dateTimeOffset = new DateTimeOffset(timestamp);
            
            // Format as ISO 8601 string with 'Z' indicator for UTC
            string formattedDateTime = dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            _logger?.LogInformation($"Sending message with timestamp: {formattedDateTime}");

            // Create message data object
            var messageData = new
            {
                createdDateTime = formattedDateTime,
                from = new
                {
                    user = new
                    {
                        id = _userId,
                        displayName = "User Name", // This should come from configuration or be dynamically generated
                        userIdentityType = "aadUser"
                    }
                },
                body = new
                {
                    contentType = "html",
                    content = messageContent
                }
            };

            return await SimpleGraphClient.PostMessageWithTimestamp(accessToken, conversationInfo.Type, id, messageData);
        }

        public async Task<HttpResponseMessage> GetConversationDetailsAsync(ConversationInfo conversationInfo)
        {
            var accessToken = await GetAccessTokenAsync();
            string id = GetAppropriateId(conversationInfo);

            return await SimpleGraphClient.GetConversationDetails(accessToken, conversationInfo.Type, id);
        }

        private string GetAppropriateId(ConversationInfo conversationInfo)
        {
            if (conversationInfo.Type == "channel")
            {
                // For channel operations, we need both teamId and channelId
                return $"{conversationInfo.TeamId}|{conversationInfo.ChannelId}";
            }
            else if (conversationInfo.Type == "groupchat")
            {
                return conversationInfo.ChatId;
            }
            
            throw new ArgumentException($"Cannot determine appropriate ID for conversation type: {conversationInfo.Type}");
        }
    }

    public class ConversationInfo
    {
        public string Type { get; set; } = "unknown";
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string ChatId { get; set; }
        public string ConversationId { get; set; }
    }
}