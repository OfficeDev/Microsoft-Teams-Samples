// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using AdaptiveCards.Templating;
using System;

namespace Microsoft.BotBuilderSamples
{
    // This bot will respond to the user's input with suggested actions.
    // Suggested actions enable your bot to present buttons that the user
    // can tap to provide input. 
    public class AdaptiveCardActionsBot : ActivityHandler
    {
        private const string CommandString = "Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card";

        /// <summary>
        /// provide logic for when members other than the bot join the conversation
        /// </summary>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Send a welcome message to the user and tell them what actions they may perform to use this bot
            var welcomeText = "Hello and Welcome!";

            // Sends an activity to the sender of the incoming activity.
            await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text(CommandString), cancellationToken);
        }

        /// <summary>
        /// provide logic specific to Message activities,
        /// </summary>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                // Extract the text from the message activity the user sent.
                var text = turnContext.Activity.Text.ToLowerInvariant();

                if (text.Contains("card actions"))
                {
                    await SendAdaptiveCardAsync(turnContext, cancellationToken, "AdaptiveCardActions.json");
                }
                else if (text.Contains("suggested actions"))
                {
                    // Respond to the user.
                    await turnContext.SendActivityAsync("Please Enter a color from the suggested action choices", cancellationToken: cancellationToken);
                    await SendAdaptiveCardAsync(turnContext, cancellationToken, "SuggestedActions.json");
                    // Sends a suggested action card
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
                else if (text.Contains("togglevisibility"))
                {
                    await SendAdaptiveCardAsync(turnContext, cancellationToken, "ToggleVisibleCard.json");
                }
                else if (text.Contains("red") || text.Contains("blue") || text.Contains("yellow"))
                {
                    var responseText = ProcessInput(text);
                    await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(CommandString), cancellationToken);
                }
            }
            await SendDataOnCardActionsAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// ProcessInput takes input string and returns message
        /// </summary>
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

        /// <summary>
        /// Creates and sends an activity with suggested actions to the user. When the user
        /// clicks one of the buttons the text value from the "CardAction" will be
        /// displayed in the channel just as if the user entered the text. There are multiple
        /// "ActionTypes" that may be used for different situations.
        /// </summary>
        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("What is your favorite color?");
            reply.SuggestedActions = new SuggestedActions
            {
                Actions = new List<CardAction>
                    {
                        new CardAction { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                        new CardAction { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                        new CardAction { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" }
                    },
                To = new List<string> { turnContext.Activity.From.Id }
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// sends the response on card action.submit
        /// </summary>
        private async Task SendDataOnCardActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                var reply = MessageFactory.Text($"Data Submitted: {turnContext.Activity.Value}");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        /// <summary>
        /// Sends an adaptive card to the user.
        /// </summary>
        private async Task SendAdaptiveCardAsync(ITurnContext turnContext, CancellationToken cancellationToken, string cardFileName)
        {
            string[] path = { ".", "Cards", cardFileName };
            var adaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCard), cancellationToken);
        }

        /// <summary>
        /// Get the initial card
        /// </summary>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            // "Expand" the template - this generates the final Adaptive Card payload
            var cardJsonString = template.Expand(payloadData);
            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString)
            };
        }
    }
}