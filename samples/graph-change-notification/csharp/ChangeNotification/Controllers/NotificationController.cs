using ChangeNotification.Helper;
using ChangeNotification.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private ResourceData resourceData = null;

        public NotificationsController(IConfiguration config,
            ILogger<NotificationsController> logger,
            IBotFrameworkHttpAdapter adapter,
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

                Console.WriteLine(content);
                resourceData = new ResourceData();
                foreach (var notification in notifications.Items)
                {
                    resourceData = notification.ResourceData;
                }

                foreach (var conversationReference in _conversationReferences.Values)
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default);
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