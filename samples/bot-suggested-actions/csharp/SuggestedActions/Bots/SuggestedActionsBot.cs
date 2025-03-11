// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This bot will respond to the user's input with suggested actions.
    /// Suggested actions enable your bot to present buttons that the user
    /// can tap to provide input.
    /// </summary>
    public class SuggestedActionsBot : ActivityHandler
    {
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
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
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
            const string colorText = "is the best color, I agree.";
            return text switch
            {
                "red" => $"Red {colorText}",
                "yellow" => $"Yellow {colorText}",
                "blue" => $"Blue {colorText}",
                _ => "Please select a color from the suggested action choices",
            };
        }

        /// <summary>
        /// Creates and sends an activity with suggested actions to the user.
        /// When the user clicks one of the buttons, the text value from the "CardAction" will be
        /// displayed in the channel just as if the user entered the text.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("What is your favorite color?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                        new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                        new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" },
                    }
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}