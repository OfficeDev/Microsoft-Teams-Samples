// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GraphChatMigration.GraphClient;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GraphChatMigration.Bot;

public class ChatMigrationBot : AgentApplication
{
    private readonly GraphHelper _graphHelper;
    private readonly ILogger<ChatMigrationBot> _logger;

    public ChatMigrationBot(AgentApplicationOptions options, IConfiguration configuration, ILogger<ChatMigrationBot> logger) 
        : base(options)
    {
        _graphHelper = new GraphHelper(configuration);
        _logger = logger;

        // Register activity handlers
        OnConversationUpdate(ConversationUpdateEvents.MembersAdded, WelcomeMessageAsync);
        OnActivity(ActivityTypes.Message, OnMessageAsync);
    }

    protected async Task WelcomeMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(MessageFactory.Text(
            "Welcome to the Migration Bot! \n\nI can help you start the chat migration process. " +
            "Type \"startMigration\" to begin, or \"help\" to see all available commands."), 
            cancellationToken);
    }
    
    // Override OnMessageAsync to handle message activities
    protected async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        try 
        {
            // Check if this is an adaptive card submission
            if (turnContext.Activity.Value != null)
            {
                await HandleAdaptiveCardSubmissionAsync(turnContext, turnState, cancellationToken);
                return;
            }

            var messageText = turnContext.Activity.Text?.Trim();
            
            // Remove bot mention from message text if present
            if (!string.IsNullOrEmpty(messageText))
            {
                // Simple approach to remove @mentions
                messageText = messageText.Replace("<at>", "").Replace("</at>", "");
                
                // Find the first space after a potential mention
                int firstSpaceIndex = messageText.IndexOf(' ');
                if (firstSpaceIndex > 0)
                {
                    // Check if the text before the space might be a mention
                    string potentialMention = messageText.Substring(0, firstSpaceIndex).Trim();
                    if (potentialMention.Contains(turnContext.Activity.Recipient.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        messageText = messageText.Substring(firstSpaceIndex).Trim();
                    }
                }
            }

            // Convert to lowercase for command matching
            var command = messageText?.ToLowerInvariant() ?? string.Empty;

            switch(command)
            {
                case "help":
                    await HandleHelpCommand(turnContext, cancellationToken);
                    break;
                case "startmigration":
                    await HandleStartMigrationCommand(turnContext, cancellationToken);
                    break;
                case "postmessage":
                    await HandlePostMessageCommand(turnContext, cancellationToken);
                    break;
                case "completemigration":
                    await HandleCompleteMigrationCommand(turnContext, cancellationToken);
                    break;
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        "I'm sorry, I didn't understand that command. Type 'help' to see available commands."), 
                        cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnMessageAsync");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"An error occurred: {ex.Message}"), cancellationToken);
        }
    }

    private async Task HandleHelpCommand(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(MessageFactory.Text(
            "Migration Bot Commands:\n\n" +
            "• startMigration - Initiates the chat migration process\n" +
            "• postMessage - Shows a form to post a message with a specific timestamp\n" +
            "• completeMigration - Completes the migration process\n" +
            "• help - Shows this help message\n\n" +
            "Type any of these commands to get started!"), 
            cancellationToken);
    }

    private async Task HandleStartMigrationCommand(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        try
        {
            // Log channel data to debug
            if (turnContext.Activity.ChannelData is JsonElement channelDataJson)
            {
                _logger.LogInformation($"Channel data: {channelDataJson.ToString()}");
            }
            else if (turnContext.Activity.ChannelData != null)
            {
                _logger.LogInformation($"Channel data type: {turnContext.Activity.ChannelData.GetType().FullName}");
            }
            
            // Get conversation info from the basic context
            var conversationInfo = _graphHelper.GetConversationInfo(
                turnContext.Activity.Conversation.Id,
                turnContext.Activity.ChannelData);

            if (conversationInfo.Type == "unknown" || string.IsNullOrEmpty(conversationInfo.ConversationId))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    "Unable to retrieve conversation information from the context."), 
                    cancellationToken);
                return;
            }

            // If this is a channel, try to get the team ID using the enhanced extraction method
            if (conversationInfo.Type == "channel")
            {
                string teamId = await _graphHelper.ExtractTeamIdFromTurnContextAsync(turnContext);
                if (!string.IsNullOrEmpty(teamId))
                {
                    _logger.LogInformation($"Retrieved team ID from context: {teamId}");
                    conversationInfo.TeamId = teamId;
                    
                    // Show the user the team ID we found for debugging purposes
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"Using Team ID: {teamId} for migration"), 
                        cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve team ID from context");
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        "Warning: Could not retrieve Team ID. Migration might fail."), 
                        cancellationToken);
                }
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("Starting migration..."), cancellationToken);

            // Call Graph API to start migration
            var response = await _graphHelper.StartMigrationAsync(conversationInfo);

            if (response.IsSuccessStatusCode)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Migration started successfully for {conversationInfo.Type}!"), 
                    cancellationToken);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("StartMigration failed: {StatusCode} - {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Failed to start migration: {response.StatusCode} - {errorContent}"), 
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartMigration command failed");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"Failed to start migration: {ex.Message}"), 
                cancellationToken);
        }
    }

    private async Task HandlePostMessageCommand(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        try
        {
            // Create a JSON-based adaptive card
            var cardJson = new JsonObject
            {
                ["type"] = "AdaptiveCard",
                ["version"] = "1.4",
                ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json",
                ["speak"] = "Please provide a timestamp and message to post.",
                ["body"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "TextBlock",
                        ["text"] = "Post Message with Timestamp",
                        ["weight"] = "Bolder",
                        ["size"] = "Medium",
                        ["color"] = "Accent"
                    },
                    new JsonObject
                    {
                        ["type"] = "TextBlock",
                        ["text"] = "Please provide the timestamp and message details:",
                        ["wrap"] = true,
                        ["spacing"] = "Medium"
                    },
                    new JsonObject
                    {
                        ["type"] = "Input.Date",
                        ["id"] = "messageDate",
                        ["label"] = "Message Date",
                        ["isRequired"] = true,
                        ["errorMessage"] = "Please select a date."
                    },
                    new JsonObject
                    {
                        ["type"] = "Input.Time",
                        ["id"] = "messageTime",
                        ["label"] = "Message Time",
                        ["isRequired"] = true,
                        ["errorMessage"] = "Please select a time.",
                        ["increment"] = 5
                    },
                    new JsonObject
                    {
                        ["type"] = "Input.Text",
                        ["id"] = "messageContent",
                        ["label"] = "Message Content",
                        ["placeholder"] = "Enter the message content...",
                        ["isMultiline"] = true,
                        ["maxLength"] = 1000,
                        ["isRequired"] = true,
                        ["errorMessage"] = "Please enter a message."
                    }
                },
                ["actions"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "Action.Submit",
                        ["title"] = "Post Message",
                        ["data"] = new JsonObject
                        {
                            ["action"] = "submitPostMessage"
                        }
                    },
                    new JsonObject
                    {
                        ["type"] = "Action.Submit",
                        ["title"] = "Cancel",
                        ["data"] = new JsonObject
                        {
                            ["action"] = "cancel"
                        }
                    }
                }
            };

            // Create an attachment with the adaptive card
            var attachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = cardJson
            };

            var activity = MessageFactory.Attachment(attachment);
            activity.Text = "Please fill out the form to post a message with a specific timestamp.";

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostMessage command failed");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"Failed to show post message form: {ex.Message}"), 
                cancellationToken);
        }
    }

    private async Task HandleCompleteMigrationCommand(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        try
        {
            // Log channel data to debug
            if (turnContext.Activity.ChannelData is JsonElement channelDataJson)
            {
                _logger.LogInformation($"Channel data: {channelDataJson.ToString()}");
            }
            
            // Get conversation info from the basic context
            var conversationInfo = _graphHelper.GetConversationInfo(
                turnContext.Activity.Conversation.Id,
                turnContext.Activity.ChannelData);

            if (conversationInfo.Type == "unknown" || string.IsNullOrEmpty(conversationInfo.ConversationId))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    "Unable to retrieve conversation information from the context."), 
                    cancellationToken);
                return;
            }

            // If this is a channel, try to get the team ID using the enhanced extraction method
            if (conversationInfo.Type == "channel" && string.IsNullOrEmpty(conversationInfo.TeamId))
            {
                string teamId = await _graphHelper.ExtractTeamIdFromTurnContextAsync(turnContext);
                if (!string.IsNullOrEmpty(teamId))
                {
                    _logger.LogInformation($"Retrieved team ID from context: {teamId}");
                    conversationInfo.TeamId = teamId;
                    
                    // Show the user the team ID we found for debugging purposes
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"Using Team ID: {teamId} for migration"), 
                        cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve team ID from context");
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        "Warning: Could not retrieve Team ID. Migration might fail."), 
                        cancellationToken);
                }
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("Completing migration..."), cancellationToken);

            // Call Graph API to complete migration
            var response = await _graphHelper.CompleteMigrationAsync(conversationInfo);

            if (response.IsSuccessStatusCode)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Migration completed successfully for {conversationInfo.Type}!"), 
                    cancellationToken);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("CompleteMigration failed: {StatusCode} - {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Failed to complete migration: {response.StatusCode} - {errorContent}"), 
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CompleteMigration command failed");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"Failed to complete migration: {ex.Message}"), 
                cancellationToken);
        }
    }

    private async Task HandleAdaptiveCardSubmissionAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling adaptive card submission");
            
            // Get the data from the adaptive card
            var data = turnContext.Activity.Value;
            if (data == null)
                return;

            // Parse action property
            string action = null;
            if (data is JsonElement jsonElement && jsonElement.TryGetProperty("action", out JsonElement actionElement))
            {
                action = actionElement.GetString();
            }
            else if (data is JsonNode jsonNode)
            {
                action = jsonNode["action"]?.GetValue<string>();
            }
            else if (data is Dictionary<string, object> dict && dict.TryGetValue("action", out object actionObj))
            {
                action = actionObj?.ToString();
            }

            // Handle different actions
            if (action == "cancel")
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    "Post message operation cancelled."), 
                    cancellationToken);
                return;
            }
            else if (action == "submitPostMessage")
            {
                await HandlePostMessageSubmission(turnContext, data, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handle adaptive card submission failed");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"Failed to process request: {ex.Message}"), 
                cancellationToken);
        }
    }

    private async Task HandlePostMessageSubmission(ITurnContext turnContext, object data, CancellationToken cancellationToken)
    {
        string messageDate = null;
        string messageTime = null;
        string messageContent = null;

        // Extract values from the data object
        if (data is JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty("messageDate", out JsonElement dateElement))
                messageDate = dateElement.GetString();
            
            if (jsonElement.TryGetProperty("messageTime", out JsonElement timeElement))
                messageTime = timeElement.GetString();
            
            if (jsonElement.TryGetProperty("messageContent", out JsonElement contentElement))
                messageContent = contentElement.GetString();
        }
        else if (data is JsonNode jsonNode)
        {
            messageDate = jsonNode["messageDate"]?.GetValue<string>();
            messageTime = jsonNode["messageTime"]?.GetValue<string>();
            messageContent = jsonNode["messageContent"]?.GetValue<string>();
        }
        else if (data is Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("messageDate", out object dateObj))
                messageDate = dateObj?.ToString();
            
            if (dict.TryGetValue("messageTime", out object timeObj))
                messageTime = timeObj?.ToString();
            
            if (dict.TryGetValue("messageContent", out object contentObj))
                messageContent = contentObj?.ToString();
        }

        // Validate required fields
        if (string.IsNullOrEmpty(messageDate) || string.IsNullOrEmpty(messageTime) || string.IsNullOrEmpty(messageContent))
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(
                "Please fill in all required fields."), 
                cancellationToken);
            return;
        }

        try
        {
            // Log the raw input values for debugging
            _logger.LogInformation($"Raw date: {messageDate}, Raw time: {messageTime}");
            
            // Log channel data to debug
            if (turnContext.Activity.ChannelData is JsonElement channelDataJson)
            {
                _logger.LogInformation($"Channel data: {channelDataJson.ToString()}");
            }
            
            // Ensure time has seconds
            if (!messageTime.Contains(":"))
            {
                messageTime = $"{messageTime}:00";
            }
            else if (messageTime.Split(':').Length == 2)
            {
                messageTime = $"{messageTime}:00";
            }
            
            // Combine date and time for Graph API
            var combinedDateTime = $"{messageDate}T{messageTime}Z"; // Adding Z for UTC timezone
            
            // Try to parse as DateTimeOffset to ensure proper format
            if (!DateTimeOffset.TryParse(combinedDateTime, out DateTimeOffset formattedTimestamp))
            {
                // If parsing fails, try creating a DateTime and converting to DateTimeOffset
                if (!DateTime.TryParse($"{messageDate} {messageTime}", out DateTime dateTime))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        "Invalid date or time format. Please try again with format YYYY-MM-DD for date and HH:MM for time."), 
                        cancellationToken);
                    return;
                }
                
                // Convert DateTime to DateTimeOffset
                formattedTimestamp = new DateTimeOffset(dateTime, TimeSpan.Zero);
            }
            
            // Log the formatted timestamp
            _logger.LogInformation($"Formatted timestamp: {formattedTimestamp.ToString("o")}");

            // Get conversation info from the basic context
            var conversationInfo = _graphHelper.GetConversationInfo(
                turnContext.Activity.Conversation.Id,
                turnContext.Activity.ChannelData);

            if (conversationInfo.Type == "unknown" || string.IsNullOrEmpty(conversationInfo.ConversationId))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    "Unable to retrieve conversation information from the context."), 
                    cancellationToken);
                return;
            }

            // If this is a channel, try to get the team ID using the enhanced extraction method
            if (conversationInfo.Type == "channel" && string.IsNullOrEmpty(conversationInfo.TeamId))
            {
                string teamId = await _graphHelper.ExtractTeamIdFromTurnContextAsync(turnContext);
                if (!string.IsNullOrEmpty(teamId))
                {
                    _logger.LogInformation($"Retrieved team ID from context: {teamId}");
                    conversationInfo.TeamId = teamId;
                    
                    // Show the user the team ID we found for debugging purposes
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"Using Team ID: {teamId} for posting message"), 
                        cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve team ID from context");
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        "Warning: Could not retrieve Team ID. Post message might fail."), 
                        cancellationToken);
                }
            }

            // Call Graph API to post message with timestamp
            var response = await _graphHelper.PostMessageWithTimestampAsync(
                conversationInfo, 
                messageContent, 
                formattedTimestamp.DateTime);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                string messageId = "";
                
                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.TryGetProperty("id", out JsonElement idElement))
                    {
                        messageId = idElement.GetString();
                    }
                }

                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Message posted successfully to {conversationInfo.Type}! Message ID: {messageId}"), 
                    cancellationToken);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("PostMessage failed: {StatusCode} - {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Failed to post message: {response.StatusCode} - {errorContent}"), 
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostMessage submission failed");
            await turnContext.SendActivityAsync(MessageFactory.Text(
                $"Failed to post message: {ex.Message}"), 
                cancellationToken);
        }
    }
}
