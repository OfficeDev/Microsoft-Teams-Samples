using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Bogus;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    /// <summary>
    /// A class for handling Teams messaging extension requests.
    /// </summary>
    public class MessageExtension : TeamsActivityHandler
    {
        // Static Faker instance to generate fake data.
        private static readonly Faker Faker = new Faker();

        /// <summary>
        /// Handles incoming message activity and replies with the same text.
        /// </summary>
        /// <param name="turnContext">The context for the turn of conversation.</param>
        /// <param name="cancellationToken">Token for canceling the operation.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Clean the activity text by removing the recipient mention.
            turnContext.Activity.RemoveRecipientMention();

            // Process the text to make it lowercase and remove leading/trailing spaces.
            var text = turnContext.Activity.Text.Trim().ToLower();

            // Send back the same text as a reply.
            var replyText = $"You said: {text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        /// <summary>
        /// Handles incoming query activity for the messaging extension.
        /// Responds to "getRandomText" query.
        /// </summary>
        /// <param name="turnContext">The context for the turn of conversation.</param>
        /// <param name="query">The query from Teams.</param>
        /// <param name="cancellationToken">Token for canceling the operation.</param>
        /// <returns>Task representing the asynchronous operation, containing the MessagingExtensionResponse.</returns>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // Ensure that only "getRandomText" command is processed
            if (query == null || query.CommandId != "getRandomText")
            {
                throw new NotImplementedException($"Invalid CommandId: {query.CommandId}");
            }

            // Retrieve cardTitle parameter if present, otherwise use an empty string.
            var title = query.Parameters?.FirstOrDefault(p => p.Name == "cardTitle")?.Value.ToString() ?? string.Empty;

            // Generate 5 attachments with random text and images.
            var attachments = GenerateRandomAttachments(title);

            // Construct the response with the generated attachments.
            var result = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments.ToList()
                }
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Generates a list of random attachments with fake data.
        /// </summary>
        /// <param name="title">The title for the card (if available).</param>
        /// <returns>List of generated messaging extension attachments.</returns>
        private static IEnumerable<MessagingExtensionAttachment> GenerateRandomAttachments(string title)
        {
            return Enumerable.Range(0, 5)
                             .Select(_ => GetAttachment(title))
                             .ToArray();
        }

        /// <summary>
        /// Creates a single random attachment with a title, text, and image.
        /// </summary>
        /// <param name="title">The title for the card.</param>
        /// <returns>A MessagingExtensionAttachment object.</returns>
        private static MessagingExtensionAttachment GetAttachment(string title)
        {
            // Create a ThumbnailCard with either the provided title or a random one.
            var card = new ThumbnailCard
            {
                Title = string.IsNullOrWhiteSpace(title) ? Faker.Lorem.Sentence() : title,
                Text = Faker.Lorem.Paragraph(),
                Images = new List<CardImage> { new CardImage($"http://lorempixel.com/640/480?rand={DateTime.Now.Ticks}") }
            };

            return card.ToAttachment().ToMessagingExtensionAttachment();
        }

        /// <summary>
        /// Handles selection of a particular item from the messaging extension.
        /// </summary>
        /// <param name="turnContext">The context for the turn of conversation.</param>
        /// <param name="query">The selected item details from the messaging extension.</param>
        /// <param name="cancellationToken">Token for canceling the operation.</param>
        /// <returns>Task representing the asynchronous operation, containing the MessagingExtensionResponse.</returns>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // Create a default attachment as a placeholder.
            var attachment = new ThumbnailCard()
                             .ToAttachment()
                             .ToMessagingExtensionAttachment();

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new[] { attachment }
                }
            });
        }
    }
}
