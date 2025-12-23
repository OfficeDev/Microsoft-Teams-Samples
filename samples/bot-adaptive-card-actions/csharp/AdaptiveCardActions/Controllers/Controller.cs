// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Common;

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
            // Build the innermost card for the nested ShowCard action
            var nestedCard = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Welcome To New Card")
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new SubmitAction
                    {
                        Title = "Click Me",
                        Data = new Union<string, SubmitActionData>("{\"value\": \"Button has Clicked\"}")
                    }
                }
            };

            // Build the middle card for the Action ShowCard
            var showCard = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("This card's action will show another card")
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new ShowCardAction
                    {
                        Title = "Action.ShowCard",
                        Card = nestedCard
                    }
                }
            };

            // Build the card for Action Submit with input field
            var submitCard = new AdaptiveCard
            {
                Version = new Microsoft.Teams.Cards.Version("1.5"),
                Body = new List<CardElement>
                {
                    new TextInput
                    {
                        Id = "name",
                        Label = "Please enter your name:",
                        IsRequired = true,
                        ErrorMessage = "Name is required"
                    }
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new SubmitAction { Title = "Submit" }
                }
            };

            // Build the main card with all actions
            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Adaptive Card Actions")
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new OpenUrlAction("https://adaptivecards.io")
                    {
                        Title = "Action Open URL"
                    },
                    new ShowCardAction
                    {
                        Title = "Action Submit",
                        Card = submitCard
                    },
                    new ShowCardAction
                    {
                        Title = "Action ShowCard",
                        Card = showCard
                    }
                }
            };

            await client.Send(card);
        }

        // Sends the Suggested Actions card using Microsoft.Teams.Cards API
        private async Task SendSuggestedActionsCardAsync(IContext.Client client)
        {
            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("**Welcome to bot Suggested actions**"),
                    new TextBlock("please use below commands, to get response form the bot."),
                    new TextBlock("- Red \r- Blue \r - Yellow")
                    {
                        Wrap = true
                    }
                }
            };

            await client.Send(card);
        }

        // Sends the Toggle Visibility card using Microsoft.Teams.Cards API
        private async Task SendToggleVisibilityCardAsync(IContext.Client client)
        {
            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("**Action.ToggleVisibility example**: click the button to show or hide a welcome message"),
                    new TextBlock("**Hello World!**")
                    {
                        Id = "helloWorld",
                        IsVisible = false,
                        Size = new TextSize("extraLarge")
                    }
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new ToggleVisibilityAction
                    {
                        Title = "Click me!",
                        TargetElements = new Union<IList<string>, IList<TargetElement>>((IList<string>)new List<string> { "helloWorld" })
                    }
                }
            };

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