// <copyright file="NotificationController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using TabActivityFeed.Models;
    using TabActivityFeed.Providers;

    /// <summary>
    /// Controller to manage subscriptions.
    /// </summary>
    public class NotificationController : Controller
    {
        // Configuration settings from appsettings.json
        private readonly IConfiguration configuration;

        // Variable representing the subscription provider.
        private readonly ISubscriptionProvider subscriptionProvider;

        // Variable representing the change notification provider.
        private readonly IChangeNotificationProvider changeNotificationProvider;

        // Variable representing the auth provider.
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="configuration">Represents the configuration settings.</param>
        /// <param name="subscriptionProvider">Represents the subscription provider.</param>
        /// <param name="changeNotificationProvider">Represents the change notification provider.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        /// <exception cref="ArgumentNullException">Represents the exception thrown when arguments are null.</exception>
        public NotificationController(IConfiguration configuration, ISubscriptionProvider subscriptionProvider, IChangeNotificationProvider changeNotificationProvider, IAuthProvider authProvider)
        {
            this.configuration = configuration;
            this.subscriptionProvider = subscriptionProvider ?? throw new ArgumentNullException(nameof(subscriptionProvider));
            this.changeNotificationProvider = changeNotificationProvider ?? throw new ArgumentNullException(nameof(changeNotificationProvider));
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        }

        /// <summary>
        /// Generates a subscription to all chats where a certain app is installed.
        /// </summary>
        /// <returns>Returns the subscription details.</returns>
        [HttpPost]
        [Route("Subscriptions/createChatSubscription")]
        public async Task<Subscription> CreateChatSubscriptionAsync()
        {
            Subscription subscription = await this.subscriptionProvider.CreateChatSubscriptionAsync().ConfigureAwait(false);

            return subscription;
        }

        /// <summary>
        /// Generates a subscription to all teams messages within a tenant.
        /// </summary>
        /// <returns>The subscription details.</returns>s
        [HttpPost]
        [Route("Subscriptions/createTeamSubscription")]
        public async Task<Subscription> CreateTeamSubscriptionAsync()
        {
            Subscription subscription = await this.subscriptionProvider.CreateTeamsSubscriptionAsync().ConfigureAwait(false);

            return subscription;
        }

        /// <summary>
        /// Processes all change notifications from the subscribed chat resource.
        /// </summary>
        /// <param name="validationToken">Represents the validation token passed from Graph API.</param>
        /// <returns>An HTTP response status code.</returns>
        [HttpPost]
        [Route("Subscriptions/processNotification")]
        public async Task<IActionResult> ProcessChangeNotificationAsync([FromQuery] string validationToken)
        {
            /*
             * Conditional is executed after subscription.
             * Must return validation back to Graph API for security measures.
             */
            if (validationToken != null)
            {
                return this.Content(validationToken, "text/plain");
            }

            // Read request details.
            using var reader = new StreamReader(this.Request.Body);
            string stringPayload = await reader.ReadToEndAsync().ConfigureAwait(false);

            // Deserialize it.
            PagedNotificationPayload pagedNotificationPayload = JsonConvert.DeserializeObject<PagedNotificationPayload>(stringPayload);

            string notificationResource = pagedNotificationPayload.Value.First().Resource;
            bool isChatResource = notificationResource.StartsWith("chats");

            if (isChatResource)
            {
                // Process chats notification payload.
                await this.changeNotificationProvider.ProcessChatsChangeNotificationAsync(pagedNotificationPayload).ConfigureAwait(false);
            }
            else
            {
                // Process teams notification payload.
                await this.changeNotificationProvider.ProcessTeamsChangeNotificationAsync(pagedNotificationPayload).ConfigureAwait(false);
            }

            // Return status code.
            return new StatusCodeResult((int)HttpStatusCode.OK);
        }
    }
}
