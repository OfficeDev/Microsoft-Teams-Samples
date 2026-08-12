using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using TeamsToDoAppConnector.Models;
using TeamsToDoAppConnector.Models.Configuration;
using TeamsToDoAppConnector.Repository;
using TeamsToDoAppConnector.Utils;

namespace TeamsToDoAppConnector.Controllers
{
    /// <summary>
    /// Represents the controller responsible for setting up the connector.
    /// </summary>
    [Authorize]
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
                // Validate the destination before storing or calling it to prevent SSRF (CWE-918).
                if (!WebhookValidator.IsValid(webhookInfo.WebhookUrl, appSettings.Value.AllowedWebhookHostSuffixes, out _))
                {
                    return RedirectToAction("Error");
                }

                // Bind the webhook to the authenticated connector context (owner + tenant).
                var ownerObjectId = User.GetObjectId();
                var ownerTenantId = User.GetTenantId();

                var subscription = SubscriptionRepository.Subscriptions
                    .Where(sub => sub.WebHookUri == webhookInfo.WebhookUrl
                        && sub.OwnerObjectId == ownerObjectId
                        && sub.OwnerTenantId == ownerTenantId)
                    .FirstOrDefault();
                if (subscription == null)
                {
                    Subscription newSubscription = new Subscription
                    {
                        WebHookUri = webhookInfo.WebhookUrl,
                        EventType = webhookInfo.EventType,
                        OwnerObjectId = ownerObjectId,
                        OwnerTenantId = ownerTenantId
                    };

                    // Save the subscription so that it can be used to push data to the registered channels.
                    SubscriptionRepository.Subscriptions.Add(newSubscription);
                }
                else
                {
                    // Update existing
                    subscription.EventType = webhookInfo.EventType;
                }

                await TaskHelper.PostWelcomeMessage(webhookInfo.WebhookUrl, appSettings.Value.BaseUrl, appSettings.Value.AllowedWebhookHostSuffixes);

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
