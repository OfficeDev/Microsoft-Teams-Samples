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
        public string commandString = "Please use one of these commands: **Card Actions** for  Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card";

        /// <summary>
        /// provide logic for when members other than the bot join the conversation
        /// </summary>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Send a welcome message to the user and tell them what actions they may perform to use this bot
            var welcomeText = "Hello and Welcome!";

            // Sends an activity to the sender of the incoming activity.
            await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);

            await turnContext.SendActivityAsync(MessageFactory.Text(commandString), cancellationToken);
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
                    string[] path = { ".", "Cards", "AdaptiveCardActions.json" };
                    var adaptiveCardForPersonalScope = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardForPersonalScope), cancellationToken);
                }
                else if (text.Contains("suggested actions"))
                {
                    //Respond to the user.
                    await turnContext.SendActivityAsync("Please Enter a color from the suggested action choices", cancellationToken: cancellationToken);

                    string[] path = { ".", "Cards", "SuggestedActions.json" };
                    var adaptiveCardForPersonalScope = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name);

                    // Sends an activity to the sender of the incoming activity.
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardForPersonalScope), cancellationToken);

                    //sends a suggested action card
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);

                }
                else if (text.Contains("togglevisibility"))
                {
                    string[] path = { ".", "Cards", "ToggleVisibleCard.json" };
                    var adaptiveCardForPersonalScope = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardForPersonalScope), cancellationToken);
                }
                else if (text.Contains("red") || (text.Contains("blue")) || text.Contains("yellow"))
                {
                    var responseText = ProcessInput(text);
                    await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(commandString), cancellationToken);
                }
            }
            await SendDataOnCardActions(turnContext, cancellationToken);
        }

        /// <summary>
        /// ProcessInput takes input string and returns message
        /// <summary>
        private static string ProcessInput(string text)
        {
            const string colorText = "is the best color, I agree.";
            switch (text)
            {
                case "red":
                    {
                        return $"Red {colorText}";
                    }
                case "yellow":
                    {
                        return $"Yellow {colorText}";
                    }
                case "blue":
                    {
                        return $"Blue {colorText}";
                    }
                default:
                    {
                        return "Please select a color from the suggested action choices";
                    }
            }
        }

        /// <summary>
        /// Creates and sends an activity with suggested actions to the user. When the user
        /// clicks one of the buttons the text value from the "CardAction" will be
        /// displayed in the channel just as if the user entered the text. There are multiple
        /// "ActionTypes" that may be used for different situations.
        /// </summary>
        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var reply = MessageFactory.Text("What is your favorite color?");
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                    new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                    new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                    new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" },
                    },
                    To = new List<string> { turnContext.Activity.From.Id },
                };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        /// <summary>
        /// sends the response on card action.submit
        /// </summary>
        private async Task SendDataOnCardActions(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                var reply = MessageFactory.Text("");
                reply.Text = $"Data Submitted : {turnContext.Activity.Value}";
                await turnContext.SendActivityAsync(MessageFactory.Text(reply.Text), cancellationToken);
            }
        }

        /// <summary>
        /// Get the initial card
        /// </summary>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };

            return adaptiveCardAttachment;
        }
    }
}