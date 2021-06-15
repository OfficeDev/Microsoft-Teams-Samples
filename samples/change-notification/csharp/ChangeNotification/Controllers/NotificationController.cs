using ChangeNotification.Helper;
using ChangeNotification.Helper;
using ChangeNotification.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeNotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        private readonly IConfiguration _config;
       // private readonly string _rootPath;
        private readonly ILogger _logger;
        public static  ITurnContext stepContext;

        private readonly static ConcurrentDictionary<string, bool> _processedMessage = new ConcurrentDictionary<string, bool>(2, 64);

        public NotificationsController(IConfiguration config,
            ILogger<NotificationsController> logger,
            UserState userState)
        {
            _config = config;
            _logger = logger;
            ;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return  Json(SubscriptionManager.Subscriptions.Select(s => new
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
                ResourceData resourceData = new ResourceData();
                foreach (var notification in notifications.Items)
                {
                    resourceData = notification.ResourceData;
                }
                if (stepContext != null)
                {
                   
                    ChangeNotificationHelper changeNotificationHelper = new ChangeNotificationHelper();
                    var attachData = changeNotificationHelper.ShowAdaptiveCard("User Presence", resourceData);
                    await stepContext.SendActivityAsync(MessageFactory.Attachment(attachData));
                }

                return Ok();
            }

        }
    }
}
