using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using MeetingApp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace MeetingApp.Controllers
{
    /// <summary>
    /// Class for Proactive messages
    /// </summary>
    [Route("api/Notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        // Dependency injected dictionary for storing Conversation Data that has roster and note details.
        private readonly ConcurrentDictionary<string, ConversationData> _conversationDataReference;

        public NotifyController(IBotFrameworkHttpAdapter adapter,
            IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            ConcurrentDictionary<string, ConversationData> conversationDataReference)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _conversationDataReference = conversationDataReference;
        }

        /// <summary>
        /// Method to share the note and asset details.
        /// </summary>
        /// <param name="assetDetails"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ShareMessage([FromBody] AssetsDetails assetDetails)
        {
            try
            {
                if (assetDetails != null)
                {
                    // Getting stored conversation data reference.
                    var dataToUpdate = new ConversationData();
                    _conversationDataReference.TryGetValue("conversationData", out dataToUpdate);
                    dataToUpdate.Note = assetDetails.Message;

                    foreach (var conversationReference in _conversationReferences.Values)
                    {
                        await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                    }

                    // Let the caller know proactive messages have been sent
                    var result = new ContentResult()
                    {
                        Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                        ContentType = "text/html",
                        StatusCode = (int)HttpStatusCode.OK,
                    };

                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Callback method to send activity.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForMessage()));
        }

        /// <summary>
        /// Sample Adaptive card to send as proactive message.
        /// </summary>
        private Attachment GetAdaptiveCardForMessage()
        {
            var updatedData = new ConversationData();
            _conversationDataReference.TryGetValue("conversationData", out updatedData);

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = updatedData.Note,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = "View document",
                        Url = new Uri("https://microsoftapc.sharepoint.com/_layouts/15/sharepoint.aspx"),
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
