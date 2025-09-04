using ChangeNotification.Helper;
using ChangeNotification.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeNotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly CloudAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private ResourceData resourceData = null;

        public NotificationsController(IConfiguration config,
            ILogger<NotificationsController> logger,
            CloudAdapter adapter,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _config = config;
            _logger = logger;
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = config["MicrosoftAppId"] ?? string.Empty;
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
            if (!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation($"Received Token: '{validationToken}'");
                return Ok(validationToken);
            }

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();
                var notifications = JsonConvert.DeserializeObject<Notifications>(content);

                _logger.LogInformation("Notification content: " + content);

                foreach (var notification in notifications.Items)
                {
                    resourceData = notification.ResourceData;
                    var userId = resourceData?.Id;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        if (TokenStore.Token!=null)
                        {
                            try
                            {
                                var tokenProvider = new SimpleAccessTokenProvider(TokenStore.Token);
                                var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

                                var graphClient = new GraphServiceClient(authProvider);

                                var presence = await graphClient.Users[userId].Presence.GetAsync();
                                resourceData.Activity = presence?.Activity;
                                resourceData.Availability = presence?.Availability;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to get presence for user: {userId}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Token not found for user: {userId}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User ID not found in notification resource data.");
                    }
                }

                foreach (var conversationReference in _conversationReferences.Values)
                {
                    await _adapter.ContinueConversationAsync(
                        _appId,
                        conversationReference,
                        BotCallback,
                        default
                    );
                }

                return Ok();
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string _notication = "Change your status to get notification";
            ChangeNotificationHelper changeNotificationHelper = new ChangeNotificationHelper();
            var attachData = changeNotificationHelper.GetAvailabilityChangeCard("User Presence", resourceData);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachData));
            await turnContext.SendActivityAsync(MessageFactory.Text(_notication, _notication));
        }
    }
}