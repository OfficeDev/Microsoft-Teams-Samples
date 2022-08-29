// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Bots
{
    using ChangeNotification.Helper;
    using Microsoft.Bot.Builder.Teams;
 
    public class ChangeNotificationBot : TeamsActivityHandler
    {
        /// <summary>
        /// Manages the subscriptions created.
        /// </summary>
        private readonly SubscriptionManager subscriptionManager;
        
        public ChangeNotificationBot(SubscriptionManager subscriptionManager)
        {
           
            this.subscriptionManager = subscriptionManager;
        }
    }
}