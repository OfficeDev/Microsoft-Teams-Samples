// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using AdaptiveCards.Templating;
using System.Text.RegularExpressions;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Cards;

namespace AdaptiveCardActions.Controllers
{
    [TeamsController]
    public class Controller()
    {
        private const string CommandString = "Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card";

        // Handles incoming message activities from users
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");

            // Handle card action submissions (when activity.Value is not null)
            if (activity.Value != null)
            {
                log.Info($"Data submitted: {activity.Value}");
                await client.Send($"Data Submitted: {activity.Value}");
                return;
            }

            if (activity.Text != null)
            {
                // Remove bot mention from the message text if present
                var text = RemoveBotMention(activity.Text);
                var normalizedText = text.Trim().ToLowerInvariant();

                if (normalizedText.Contains("card actions"))
                {
                    await SendAdaptiveCardAsync(client, "AdaptiveCardActions.json", activity.From?.Name);
                }
                else if (normalizedText.Contains("suggested actions"))
                {
                    // Respond to the user before sending suggested actions
                    await client.Send("Please Enter a color from the suggested action choices");
                    await SendAdaptiveCardAsync(client, "SuggestedActions.json", activity.From?.Name);
                    // Send native Teams SDK suggested actions
                    await SendSuggestedActionsAsync(client, log);
                }
                else if (normalizedText.Contains("togglevisibility"))
                {
                    await SendAdaptiveCardAsync(client, "ToggleVisibleCard.json", activity.From?.Name);
                }
                else if (normalizedText.Contains("red") || normalizedText.Contains("blue") || normalizedText.Contains("yellow"))
                {
                    var responseText = ProcessInput(normalizedText);
                    await client.Send(responseText);
                    // Send suggested actions again after color selection
                    await SendSuggestedActionsAsync(client, log);
                }
                else
                {
                    await client.Send(CommandString);
                }
            }
        }

        // Handles when new members are added to the conversation
        [Microsoft.Teams.Apps.Activities.Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "Hello and Welcome!";
            
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient?.Id)
                {
                    await context.Send(welcomeText);
                    await context.Send(CommandString);
                }
            }
        }

        // Processes user input and returns appropriate color response
        private static string ProcessInput(string text)
        {
            const string colorText = "is the best color, I agree.";
            var colorResponses = new Dictionary<string, string>
            {
                { "red", $"Red {colorText}" },
                { "yellow", $"Yellow {colorText}" },
                { "blue", $"Blue {colorText}" }
            };

            return colorResponses.TryGetValue(text, out var response) ? response : "Please select a color from the suggested action choices";
        }

        // Loads and sends an adaptive card from the specified JSON file
        private async Task SendAdaptiveCardAsync(IContext.Client client, string cardFileName, string userName = null)
        {
            try
            {
                string cardPath = Path.Combine(".", "Cards", cardFileName);
                var adaptiveCardJson = await GetAdaptiveCardAsync(cardPath, userName);
                
                // Deserialize the card JSON into an AdaptiveCard using the provided static method
                var adaptiveCard = AdaptiveCard.Deserialize(adaptiveCardJson);

                if (adaptiveCard == null)
                {
                    await client.Send("Error: Failed to deserialize adaptive card.");
                    return;
                }

                // Send the adaptive card using the correct method
                await client.Send(adaptiveCard);
            }
            catch (Exception ex)
            {
                await client.Send($"Error sending card: {ex.Message}");
            }
        }

        // Reads adaptive card template and expands it with user data
        private async Task<string> GetAdaptiveCardAsync(string filepath, string name = null)
        {
            var adaptiveCardJson = await File.ReadAllTextAsync(filepath);
            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdBy = name ?? "User"
            };

            return template.Expand(payloadData);
        }

        /// <summary>
        /// Removes bot mention from the message text.
        /// </summary>
        private string RemoveBotMention(string text)
        {
            // Remove @BotName mentions using regex
            var mentionRegex = new Regex(@"<at[^>]*>.*?</at>\s*", RegexOptions.IgnoreCase);
            return mentionRegex.Replace(text, "").Trim();
        }

        /// <summary>
        /// Creates and sends a message with suggested actions to the user.
        /// Uses native Teams SDK SuggestedActions with IMBack action type.
        /// When the user clicks a button, the text value will be displayed in the channel as if the user typed it,
        /// and will automatically trigger the OnMessage handler.
        /// </summary>
        private async Task SendSuggestedActionsAsync(IContext.Client client, Microsoft.Teams.Common.Logging.ILogger? log)
        {
            log?.Info("Sending suggested actions");

            // Create MessageActivity with SuggestedActions using native Teams SDK
            var message = new MessageActivity()
            {
                Text = "What is your favorite color?",
                SuggestedActions = new Microsoft.Teams.Api.SuggestedActions()
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "Red",
                        Value = "Red"
                    })
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "Yellow",
                        Value = "Yellow"
                    })
                    .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
                    {
                        Title = "Blue",
                        Value = "Blue"
                    })
            };

            await client.Send(message);
        }
    }
}