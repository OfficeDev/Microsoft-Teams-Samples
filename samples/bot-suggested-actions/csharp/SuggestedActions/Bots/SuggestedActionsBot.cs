// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This bot will respond to the user's input with suggested actions.
    /// Suggested actions enable your bot to present buttons that the user
    /// can tap to provide input.
    /// </summary>
    public class SuggestedActionsBot : ActivityHandler
    {
        private readonly IConfiguration _configuration;

        public SuggestedActionsBot(IConfiguration config)
        {
            _configuration = config;
        }

        /// <summary>
        /// Invoked when members are added to the conversation.
        /// Sends a welcome message to the user and tells them what actions they may perform to use this bot.
        /// </summary>
        /// <param name="membersAdded">List of members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when a message activity is received from the user.
        /// Processes the user's input and responds with the appropriate message and suggested actions.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.ToLowerInvariant();
            var responseText = ProcessInput(text);

            await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);
            await SendSuggestedActionsAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Sends a welcome message to the user and tells them what actions they may perform to use this bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        "Welcome to the suggested actions bot. This bot will introduce you to suggested actions. Please answer the question:",
                        cancellationToken: cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Processes the user's input and returns the appropriate response text.
        /// </summary>
        /// <param name="text">The user's input text.</param>
        /// <returns>The response text based on the user's input.</returns>
        private static string ProcessInput(string text)
        {
            const string colorText = "How can I assist you today?";
            return text switch
            {
                "hello" => $"Hello, {colorText}",
                "welcome" => $"Welcome, {colorText}",
                _ => "Please select one action",
            };
        }

        /// <summary>
        /// Creates and sends an activity with suggested actions to the user.
        /// When the user clicks one of the buttons, the text value from the "CardAction" will be
        /// displayed in the channel just as if the user entered the text.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Choose one of the action from the suggested action?");

            var payload = new
            {
                    type = "Teams.chatMessage",
                    data = new
                    {
                        body = new
                        {
                            additionalData = new { },
                            backingStore = new
                            {
                                returnOnlyChangedValues = false,
                                initializationCompleted = true
                            },
                            content = "<at id=\"0\">SuggestedActionsBot</at>"
                        },
                        mentions = new[]
                        {
                            new
                            {
                                additionalData = new { },
                                backingStore = new
                                {
                                    returnOnlyChangedValues = false,
                                    initializationCompleted = false
                                },
                                id = 0,
                                mentioned = new
                                {
                                    additionalData = new { },
                                    backingStore = new
                                    {
                                        returnOnlyChangedValues = false,
                                        initializationCompleted = false
                                    },
                                    odataType = "#microsoft.graph.chatMessageMentionedIdentitySet",
                                    user = new
                                    {
                                        additionalData = new { },
                                        backingStore = new
                                        {
                                            returnOnlyChangedValues = false,
                                            initializationCompleted = false
                                        },
                                        displayName = "Suggested Actions Bot",
                                        id = "28:" + _configuration["MicrosoftAppId"],
                                    }
                                },
                                mentionText = "Suggested Actions Bot"
                            }
                        },
                        additionalData = new { },
                        backingStore = new
                        {
                            returnOnlyChangedValues = false,
                            initializationCompleted = true
                        }
                    }
            };

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Hello", Type = ActionTypes.ImBack, Value = "Hello" },
                    new CardAction() { Title = "Welcome", Type = ActionTypes.ImBack, Value = "Welcome" },
                    new CardAction() { Title = "@SuggestedActionsBot", Type = "Action.Compose", Value = payload }
                }
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}