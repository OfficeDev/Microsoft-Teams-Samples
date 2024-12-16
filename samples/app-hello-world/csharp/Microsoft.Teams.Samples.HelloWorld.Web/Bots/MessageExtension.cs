// <copyright file="MessageExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A bot that handles messaging extensions for Microsoft Teams.
    /// </summary>
    public class MessageExtension : TeamsActivityHandler
    {
        /// <summary>
        /// Handles incoming message activities.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            var replyText = $"You said: {text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        /// <summary>
        /// Handles messaging extension queries.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="query">The query object for the messaging extension.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="NotImplementedException">Thrown when the command ID is invalid.</exception>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var title = string.Empty;
            var titleParam = query.Parameters?.FirstOrDefault(p => p.Name == "cardTitle");
            if (titleParam != null)
            {
                title = titleParam.Value.ToString();
            }

            if (query == null || query.CommandId != "getRandomText")
            {
                // We only process the 'getRandomText' queries with this message extension
                throw new NotImplementedException($"Invalid CommandId: {query.CommandId}");
            }

            var attachments = new MessagingExtensionAttachment[5];

            for (int i = 0; i < 5; i++)
            {
                attachments[i] = GetAttachment(title);
            }

            var result = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments.ToList(),
                },
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Handles the selection of an item in a messaging extension.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="query">The query object for the messaging extension.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new MessagingExtensionAttachment[]
                    {
                            new ThumbnailCard()
                            .ToAttachment()
                            .ToMessagingExtensionAttachment(),
                    },
                },
            });
        }

        /// <summary>
        /// Creates a messaging extension attachment with a thumbnail card.
        /// </summary>
        /// <param name="title">The title for the card.</param>
        /// <returns>A messaging extension attachment.</returns>
        private static MessagingExtensionAttachment GetAttachment(string title)
        {
            var card = new ThumbnailCard
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title : new Faker().Lorem.Sentence(),
                Text = new Faker().Lorem.Paragraph(),
                Images = new List<CardImage> { new CardImage("http://lorempixel.com/640/480?rand=" + DateTime.Now.Ticks.ToString()) },
            };

            return card
                .ToAttachment()
                .ToMessagingExtensionAttachment();
        }
    }
}
