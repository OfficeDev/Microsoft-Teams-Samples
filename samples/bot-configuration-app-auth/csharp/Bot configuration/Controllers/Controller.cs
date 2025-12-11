// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using System.Text.Json;

namespace Bot_configuration.Controllers
{
    /// <summary>
    /// Teams SDK v2 Controller for Bot Configuration Sample
    /// Demonstrates bot configuration through settings button (config invoke activities)
    /// </summary>
    [TeamsController]
    public class Controller
    {
        private static string _chosenFlow = string.Empty;

        /// <summary>
        /// Handles conversation members added event
        /// Sends welcome message with configuration instructions
        /// </summary>
        [Conversation.MembersAdded]
        public async Task OnMembersAdded([Context] ConversationUpdateActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Members added to conversation");

            const string imagePath = "Images/configbutton.png";
            string imageData = string.Empty;

            if (File.Exists(imagePath))
            {
                imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
            }

            var card = new Microsoft.Teams.Cards.AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card.")
                    {
                        Wrap = true,
                        Size = TextSize.Large,
                        Weight = TextWeight.Bolder
                    }
                }
            };

            if (!string.IsNullOrEmpty(imageData))
            {
                card.Body.Add(new Image($"data:image/png;base64,{imageData}"));
            }

            await client.Send(card);
        }

        /// <summary>
        /// Handles incoming messages
        /// </summary>
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            if (!string.IsNullOrEmpty(activity.Text))
            {
                var text = activity.Text.ToLower().Trim();
                
                log.Info($"Received message: {activity.Text}");

                if (text == "chosen flow" || text.Contains("chosen flow"))
                {
                    var response = string.IsNullOrEmpty(_chosenFlow) 
                        ? "No flow has been configured yet. Please use the settings button to configure the bot."
                        : $"Bot configured for {_chosenFlow} flow";
                    
                    await client.Send(response);
                }
                else
                {
                    await client.Send($"You said: '{activity.Text}'\n\nTry sending 'chosen flow' to check configuration.");
                }
            }
            else if (activity.Value != null)
            {
                await client.Send($"Selected option is: {activity.Value}");
            }
        }

        /// <summary>
        /// Handles config/fetch invoke - Shows auth dialog when settings button is clicked
        /// Replaces OnTeamsConfigFetchAsync from Bot Builder SDK
        /// </summary>
        [Invoke("config/fetch")]
        public object OnConfigFetch([Context] InvokeActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Config fetch received - showing auth dialog");

            return new
            {
                config = new
                {
                    type = "auth",
                    suggestedActions = new
                    {
                        actions = new[]
                        {
                            new
                            {
                                type = "openUrl",
                                title = "Sign in to this app",
                                value = "https://example.com/auth"
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Handles config/submit invoke - Processes configuration submission
        /// Replaces OnTeamsConfigSubmitAsync from Bot Builder SDK
        /// </summary>
        [Invoke("config/submit")]
        public async Task<object> OnConfigSubmit([Context] InvokeActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Config submit received");

            try
            {
                if (activity.Value != null)
                {
                    var jsonString = JsonSerializer.Serialize(activity.Value);
                    log.Info($"Configuration data: {jsonString}");

                    var valueElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
                    if (valueElement.TryGetProperty("flow", out var flowProperty))
                    {
                        _chosenFlow = flowProperty.GetString() ?? string.Empty;
                        log.Info($"Configuration updated to: {_chosenFlow}");
                    }
                }

                await client.Send("✅ You have chosen to finish setting up bot");

                return new
                {
                    config = new
                    {
                        type = "message",
                        value = "You have chosen to finish setting up bot"
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error($"Error processing configuration: {ex.Message}");
                return new
                {
                    config = new
                    {
                        type = "message",
                        value = "An error occurred while saving the configuration."
                    }
                };
            }
        }
    }
}