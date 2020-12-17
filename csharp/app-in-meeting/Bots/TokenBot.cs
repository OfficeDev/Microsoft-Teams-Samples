// <copyright file="TokenBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Bots
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using TokenApp.Extensions;
    using TokenApp.Repository;

    /// <summary>
    /// Meeting Token Bot.
    /// </summary>
    public class TokenBot : ActivityHandler
    {
        private readonly ITenantInfoRepository tenantInfoRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenBot"/> class.
        /// </summary>
        /// <param name="tenantInfoRepository">tenant information repository.</param>
        public TokenBot(ITenantInfoRepository tenantInfoRepository)
        {
            this.tenantInfoRepository = tenantInfoRepository;
        }

        /// <inheritdoc/>
        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // NOTE: The implementation of ITenantInfoRepository included in this sample uses an in-memory store,
            // so the service URL mapping is lost when the service is restarted.
            // To set the service URL after an service restart, send the bot a message from the chat. The OnMessageActivityAsync handler
            // below will set the service URL from the information in the message activity.

            // Set the service url for the tenant based on the incoming activity when the app is installed
            var activity = turnContext.Activity;
            if (!activity.MembersAdded.IsNullOrEmpty() &&
                activity.MembersAdded.Any(acct => acct.Id == activity.Recipient.Id))
            {
                // App was installed to a chat
                this.tenantInfoRepository.SetServiceUrl(turnContext.Activity.Conversation.TenantId, turnContext.Activity.ServiceUrl);
            }

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // NOTE: This handler is included here only to make it easy to set the service URL information after a service restart.
            this.tenantInfoRepository.SetServiceUrl(turnContext.Activity.Conversation.TenantId, turnContext.Activity.ServiceUrl);

            return base.OnMessageActivityAsync(turnContext, cancellationToken);
        }
    }
}
