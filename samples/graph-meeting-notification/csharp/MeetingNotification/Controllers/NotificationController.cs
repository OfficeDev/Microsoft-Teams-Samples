// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Controllers
{
    using MeetingNotification.Helper;
    using MeetingNotification.Helpers;
    using MeetingNotification.Model;
    using MeetingNotification.Model.Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<BotConfiguration> botSettings;

        private readonly ILogger _logger;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotificationsController(IOptions<BotConfiguration> botSettings,
            ILogger<NotificationsController> logger,
            IBotFrameworkHttpAdapter adapter,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            this.botSettings = botSettings;
            _logger = logger;
            _adapter = adapter;
            _conversationReferences = conversationReferences;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Json(SubscriptionManager.Subscriptions.Select(s => new
            {
                Resource = s.Value.Resource,
                ExpirationDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(s.Value.ExpirationDateTime.Value.DateTime, "UTC", "India Standard Time").ToString("MM/dd/yyyy h:mm tt"),
                Expired = s.Value.ExpirationDateTime < DateTime.UtcNow
            }));
        }

        // Callback
        [Route("/api/webhookLifecyle")]
        [HttpPost]
        public ActionResult WebhookLifecyle([FromQuery] string validationToken = null)
        {
            if (validationToken != null)
            {
                // Ack the webhook subscription
                return Ok(validationToken);
            }
            else
            {
                _logger.LogCritical("To do -- handle authorization challenge");
                return null;
            }
        }

        public async Task<ActionResult<string>> Post([FromQuery] string validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation($"Received Token: '{validationToken}'");

                return Ok(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                var notifications = JsonConvert.DeserializeObject<Notifications>(content);

                foreach (var notification in notifications.Items)
                {
                    // Initialize with the private key that matches the encryptionCertificateId.
                    var data = DecryptionHelper.GetDecryptedContent(notification.EncryptedContent,
                                                    this.botSettings.Value.CertificateThumbprint);

                    var meetingResource = JsonConvert.DeserializeObject<MeetingResource>(data);

                    var meetingJoinUrl = GetMeetingJoinUrl(HttpUtility.UrlDecode(meetingResource.Id));

                    if (_conversationReferences.TryGetValue(meetingJoinUrl, out var meetingConversationReference))
                    {
                        if (meetingResource.EventType == MeetingNotificationType.CallUpdated )
                        {
                            var meetingResourceUpdate = JsonConvert.DeserializeObject<MeetingResourceUpdate>(data);
                            if (meetingResourceUpdate.ActiveParticipantJoined.Count > 0 || meetingResourceUpdate.ActiveParticipantLeft.Count > 0)
                            {
                                var proactiveMessage = NotificationCardHelper.GetMeetingUpdatedCard(meetingResourceUpdate);

                                await ((BotAdapter)_adapter).ContinueConversationAsync(this.botSettings.Value.MicrosoftAppId,
                                   meetingConversationReference,
                                   async (turnContext, cancellationToken) =>
                                   {
                                       await turnContext.SendActivityAsync(MessageFactory.Attachment(proactiveMessage), cancellationToken);
                                   },
                                   default);
                            }
                        }
                        else
                        {
                            var proactiveMessage = NotificationCardHelper.GetMeetingStartedEndedCard(meetingResource);
                            await ((BotAdapter)_adapter).ContinueConversationAsync(this.botSettings.Value.MicrosoftAppId,
                               meetingConversationReference,
                               async (turnContext, cancellationToken) =>
                               {
                                   await turnContext.SendActivityAsync(MessageFactory.Attachment(proactiveMessage), cancellationToken);
                               }, 
                               default);
                        }
                    }
                }

                return Ok();
            }
        }

        private string GetMeetingJoinUrl(string source)
        {
            var character = "'";
            string result = "";
            if (source.Contains(character))
            {
                int StartIndex = source.IndexOf(character, 0) + character.Length;
                int EndIndex = source.LastIndexOf(character);

                result = source.Substring(StartIndex, EndIndex - StartIndex);
                return result;
            }

            return result;
        }
    }
}