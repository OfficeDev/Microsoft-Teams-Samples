using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TeamsToDoAppConnector.Models;
using TeamsToDoAppConnector.Models.Configuration;
using TeamsToDoAppConnector.Repository;
using TeamsToDoAppConnector.Utils;

namespace TeamsToDoAppConnector.Controllers
{
    /// <summary>
    /// Represents the controller responsible for setting up the connector.
    /// </summary>
    public class ConnectorController : Controller
    {

        /// <summary>
        /// Stores the AppSettings configuration values.
        /// </summary>
        private readonly IOptions<AppSettings> appSettings;
        public ConnectorController(IOptions<AppSettings> app)
        {
            appSettings = app;
        }

        /// <summary>
        /// This is the landing page when user tries to setup the connector.
        /// You could implement login here, if required.
        /// </summary>
        public ViewResult Setup()
        {
            return View();
        }

        /// <summary>
        /// This enpoint is called when we need to save the webhook details.
        /// This contains Webhook Url and event type which can be used to push change notifications to the channel.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Save(WebhookDetails webhookInfo)
        {
            if (webhookInfo == null || webhookInfo.WebhookUrl == null)
            {
                return RedirectToAction("Error"); // You could pass error message to Error Action. 
            }
            else
            {
                var subscription = SubscriptionRepository.Subscriptions.Where(sub => sub.WebHookUri == webhookInfo.WebhookUrl).FirstOrDefault();
                if (subscription == null)
                {
                    Subscription newSubscription = new Subscription
                    {
                        WebHookUri = webhookInfo.WebhookUrl,
                        EventType = webhookInfo.EventType
                    };

                    // Save the subscription so that it can be used to push data to the registered channels.
                    SubscriptionRepository.Subscriptions.Add(newSubscription);
                }
                else
                {
                    // Update existing
                    subscription.EventType = webhookInfo.EventType;
                }

                await TaskHelper.PostWelcomeMessage(webhookInfo.WebhookUrl, appSettings.Value.BaseUrl);

                return View();
            }
        }

        // Error page
        public ActionResult Error()
        {
            return View();
        }
    }
}
