// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace PeoplePicker.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        /// <summary>
        /// Handles when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                string[] path = turnContext.Activity.Conversation.ConversationType == "personal"
                    ? new[] { ".", "Cards", "PersonalScopeCard.json" }
                    : new[] { ".", "Cards", "TeamsScopeCard.json" };

                var adaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name);

                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCard), cancellationToken);
            }
            else if (turnContext.Activity.Value != null)
            {
                // Send task details as a simple text message.
                await turnContext.SendActivityAsync(MessageFactory.Text($"Task details are: {turnContext.Activity.Value}"), cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when the bot (like a user) is added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can see the functionality of the people-picker in an adaptive card."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets the initial adaptive card.
        /// </summary>
        /// <param name="filepath">The path to the JSON file that contains the adaptive card template.</param>
        /// <param name="name">The name of the person who created the card.</param>
        /// <returns>The generated adaptive card attachment.</returns>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name)
        {
            // Async file I/O to avoid blocking the main thread
            var adaptiveCardJson = File.ReadAllTextAsync(Path.Combine(filepath));

            // Expand the template with dynamic data
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson.Result);
            var payloadData = new { createdBy = name };

            // Generate the final adaptive card JSON
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
