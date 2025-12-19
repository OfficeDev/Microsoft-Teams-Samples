// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
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
                var normalizedText = activity.Text.Trim().ToLowerInvariant();

                if (normalizedText.Contains("card actions"))
                {
                    await SendAdaptiveCardActionsAsync(client);
                }
                else if (normalizedText.Contains("suggested actions"))
                {
                    // Respond to the user before sending suggested actions
                    await client.Send("Please Enter a color from the suggested action choices");
                    await SendSuggestedActionsCardAsync(client);
                    // Send native Teams SDK suggested actions
                    await SendSuggestedActionsAsync(client, log);
                }
                else if (normalizedText.Contains("togglevisibility"))
                {
                    await SendToggleVisibilityCardAsync(client);
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

        // Sends the Adaptive Card Actions card using Microsoft.Teams.Cards API
        private async Task SendAdaptiveCardActionsAsync(IContext.Client client)
        {
            var cardJson = """
            {
              "type": "AdaptiveCard",
              "version": "1.0",
              "body": [
                {
                  "type": "TextBlock",
                  "text": "Adaptive Card Actions"
                }
              ],
              "actions": [
                {
                  "type": "Action.OpenUrl",
                  "title": "Action Open URL",
                  "url": "https://adaptivecards.io"
                },
                {
                  "type": "Action.ShowCard",
                  "title": "Action Submit",
                  "card": {
                    "type": "AdaptiveCard",
                    "version": "1.5",
                    "body": [
                      {
                        "type": "Input.Text",
                        "id": "name",
                        "label": "Please enter your name:",
                        "isRequired": true,
                        "errorMessage": "Name is required"
                      }
                    ],
                    "actions": [
                      {
                        "type": "Action.Submit",
                        "title": "Submit"
                      }
                    ]
                  }
                },
                {
                  "type": "Action.ShowCard",
                  "title": "Action ShowCard",
                  "card": {
                    "type": "AdaptiveCard",
                    "version": "1.0",
                    "body": [
                      {
                        "type": "TextBlock",
                        "text": "This card's action will show another card"
                      }
                    ],
                    "actions": [
                      {
                        "type": "Action.ShowCard",
                        "title": "Action.ShowCard",
                        "card": {
                          "type": "AdaptiveCard",
                          "body": [
                            {
                              "type": "TextBlock",
                              "text": "Welcome To New Card"
                            }
                          ],
                          "actions": [
                            {
                              "type": "Action.Submit",
                              "title": "Click Me",
                              "data": {
                                "value": "Button has Clicked"
                              }
                            }
                          ]
                        }
                      }
                    ]
                  }
                }
              ]
            }
            """;

            var card = AdaptiveCard.Deserialize(cardJson);
            await client.Send(card);
        }

        // Sends the Suggested Actions card using Microsoft.Teams.Cards API
        private async Task SendSuggestedActionsCardAsync(IContext.Client client)
        {
            var cardJson = """
            {
              "type": "AdaptiveCard",
              "version": "1.0",
              "body": [
                {
                  "type": "TextBlock",
                  "text": "**Welcome to bot Suggested actions**"
                },
                {
                  "type": "TextBlock",
                  "text": "please use below commands, to get response form the bot."
                },
                {
                  "type": "TextBlock",
                  "text": "- Red \r- Blue \r - Yellow",
                  "wrap": true
                }
              ]
            }
            """;

            var card = AdaptiveCard.Deserialize(cardJson);
            await client.Send(card);
        }

        // Sends the Toggle Visibility card using Microsoft.Teams.Cards API
        private async Task SendToggleVisibilityCardAsync(IContext.Client client)
        {
            var cardJson = """
            {
              "type": "AdaptiveCard",
              "version": "1.0",
              "body": [
                {
                  "type": "TextBlock",
                  "text": "**Action.ToggleVisibility example**: click the button to show or hide a welcome message"
                },
                {
                  "type": "TextBlock",
                  "id": "helloWorld",
                  "isVisible": false,
                  "text": "**Hello World!**",
                  "size": "extraLarge"
                }
              ],
              "actions": [
                {
                  "type": "Action.ToggleVisibility",
                  "title": "Click me!",
                  "targetElements": [ "helloWorld" ]
                }
              ]
            }
            """;

            var card = AdaptiveCard.Deserialize(cardJson);
            await client.Send(card);
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