// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using AdaptiveCards.Templating;
using AdaptiveCards;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// TeamsBot prompts a user to select a Rich Card and then returns the card
    /// that matches the user's selection.
    /// </summary>
    public class TeamsBot : DialogBot<MainDialog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsBot"/> class.
        /// </summary>
        /// <param name="conversationState">The conversation state.</param>
        /// <param name="userState">The user state.</param>
        /// <param name="dialog">The dialog to run.</param>
        /// <param name="logger">The logger instance.</param>
        public TeamsBot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<DialogBot<MainDialog>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot join the conversation, such as your bot's welcome logic.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to Card Bot. This bot will show you different types of Cards. Please type anything to get started.");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Creates an adaptive card invoke response with the provided media URL.
        /// </summary>
        /// <param name="url">URL of the media file.</param>
        /// <returns>An <see cref="InvokeResponse"/> containing the adaptive card.</returns>
        private InvokeResponse CreateAdaptiveCardInvokeResponse(string url)
        {
            var filepath = Path.Combine(".", "Resources", "adaptiveCardMedia.json");
            var adaptiveCardJson = File.ReadAllText(filepath);
            var template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new { mediaUrl = url };
            var cardJsonString = template.Expand(payloadData);
            var adaptiveCardResponse = new AdaptiveCardInvokeResponse
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = JsonConvert.DeserializeObject(cardJsonString)
            };

            return CreateInvokeResponse(adaptiveCardResponse);
        }

        /// <summary>
        /// Handles invoke activities, such as adaptive card actions.
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                if (turnContext.Activity.Value == null)
                    return null;

                var value = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());

                if (value["action"] == null)
                    return null;

                var actionData = JsonConvert.DeserializeObject<JObject>(value["action"]["data"].ToString());

                if (actionData["url"] == null)
                    return null;

                var url = actionData["url"].ToString();

                return CreateAdaptiveCardInvokeResponse(url);
            }

            return null;
        }
    }
}