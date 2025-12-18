using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Extensions.Options;
using meetings_transcription.Models.Configuration;
using meetings_transcription.Helpers;
using meetings_transcription.Services;
using meetings_transcription.Models;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace meetings_transcription.Controllers
{
    [TeamsController]
    public class Controller
    {
        /// <summary>
        /// Helper instance to make graph calls.
        /// </summary>
        private readonly GraphHelper graphHelper;

        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        /// <summary>
        /// Store details of meeting transcript.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> transcriptsDictionary;

        /// <summary>
        /// Instance of card factory to create adaptive cards.
        /// </summary>
        private readonly ICardFactory cardFactory;

        /// <summary>
        /// Creates bot instance.
        /// </summary>
        /// <param name="azureSettings">Stores the Azure configuration values.</param>
        /// <param name="transcriptsDictionary">Store details of meeting transcript.</param>
        /// <param name="cardFactory">Instance of card factory to create adaptive cards.</param>
        public Controller(IOptions<AzureSettings> azureSettings, ConcurrentDictionary<string, string> transcriptsDictionary, ICardFactory cardFactory)
        {
            this.transcriptsDictionary = transcriptsDictionary;
            this.azureSettings = azureSettings;
            this.graphHelper = new GraphHelper(azureSettings);
            this.cardFactory = cardFactory;
        }

        /// <summary>
        /// Activity handler for on message activity.
        /// </summary>
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            var replyText = $"Echo: {activity.Text}";
            await client.Send(replyText);
        }

        /// <summary>
        /// Activity handler for conversation members added.
        /// </summary>
        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "How can I help you today?";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }
    }
}
