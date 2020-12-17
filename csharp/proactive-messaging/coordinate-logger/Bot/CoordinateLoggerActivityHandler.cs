// <copyright file="msteams-app-coordinateloggerActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.CoordinateLogger.Bot
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.CoordinateLogger.Services;

    /// <summary>
    /// The CoordinateLoggerActivityHandler is responsible for reacting to incoming events from Teams sent from BotFramework.
    /// </summary>
    public sealed class CoordinateLoggerActivityHandler : TeamsActivityHandler
    {
        private readonly ILogger<CoordinateLoggerActivityHandler> logger;

        private readonly MicrosoftAppCredentials appCredentials;

        private readonly IConnectorClientFactory connectorClientFactory;

        public CoordinateLoggerActivityHandler(
            ILogger<CoordinateLoggerActivityHandler> logger, 
            MicrosoftAppCredentials appCredentials, 
            IConnectorClientFactory connectorClientFactory)
        {
            this.logger = logger;
            this.appCredentials = appCredentials;
            this.connectorClientFactory = connectorClientFactory;
        }

        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await this.HandleUserConversationUpdateAsync(turnContext);
            await this.HandleTeamUserCoordinatesAsync(turnContext);
            await this.HandleChannelThreadCoordinatesAsync(turnContext);
            
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        
        /// Get the coordinates of a user when they install the app personally.
        private Task HandleUserConversationUpdateAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var tenantId = turnContext.Activity.Conversation?.TenantId;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationId = turnContext.Activity.Conversation?.Id;

            var isPersonalConversation = turnContext.Activity.Conversation.ConversationType == "personal";
            var isInstallEvent = turnContext.Activity.MembersAdded?.Any(member => member.Id == $"28:{this.appCredentials.MicrosoftAppId}") == true;

            if (!isPersonalConversation || !isInstallEvent)
            {
                return Task.CompletedTask;
            }
        
            var teamsConnectorClient = new TeamsConnectorClient(new Uri(serviceUrl), this.appCredentials);
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            
            this.LogConversationCoordinates("User", serviceUrl, conversationId);

            return Task.CompletedTask;
        }

        /// Get the coordinates of all the users inside the Team.
        private async Task HandleTeamUserCoordinatesAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var tenantId = turnContext.Activity.Conversation?.TenantId;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationId = turnContext.Activity.Conversation?.Id;

            var isChannelConversationType = turnContext.Activity.Conversation.ConversationType == "channel";
            var isInstallEvent = turnContext.Activity.MembersAdded?.Any(member => member.Id == $"28:{this.appCredentials.MicrosoftAppId}") == true;

            // We only care about channel conversations.
            if (!isChannelConversationType)
            {
                return;
            }
            
            if (isInstallEvent)
            {
                var connectorClient = this.connectorClientFactory.GetConnectorClient(serviceUrl);
                var membersReponse = await connectorClient.Conversations.GetConversationMembersAsync(conversationId);
                var userMembers = membersReponse
                    .Where(account => account.AadObjectId != null);
                foreach (var userMember in userMembers)
                {
                    await this.CreateConversationForUserAsync(userMember, tenantId, serviceUrl);
                }
            }
            else
            {
                var userMembers = turnContext.Activity.MembersAdded
                    .Where(account => account.AadObjectId != null);
                foreach (var userMember in userMembers)
                {
                    await this.CreateConversationForUserAsync(userMember, tenantId, serviceUrl);
                }
            }
        }

        /// Create a conversation in each channel & get the coordinates to that channel thread.
        private async Task HandleChannelThreadCoordinatesAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var tenantId = turnContext.Activity.Conversation?.TenantId;
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var conversationId = turnContext.Activity.Conversation?.Id;

            var isChannelConversationType = turnContext.Activity.Conversation.ConversationType == "channel";
            var isInstallEvent = turnContext.Activity.MembersAdded?.Any(member => member.Id == $"28:{this.appCredentials.MicrosoftAppId}") == true;

            if (!isChannelConversationType)
            {
                return;
            }
        
            var teamsConnectorClient = new TeamsConnectorClient(new Uri(serviceUrl), this.appCredentials);
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var channelList = await teamsConnectorClient.Teams.FetchChannelListAsync(teamsChannelData.Team.Id);

            foreach (var channel in channelList.Conversations)
            {
                await this.CreateConversationForChannelAsync(tenantId, channel.Id, serviceUrl);
            }
        }

        private async Task CreateConversationForUserAsync(ChannelAccount user, string tenantId, string serviceUrl)
        {
            var connectorClient = this.connectorClientFactory.GetConnectorClient(serviceUrl);
            var conversationParams = new ConversationParameters
            {
                Bot = new ChannelAccount
                {
                    Id = this.appCredentials.MicrosoftAppId,
                },
                Members = new[]
                {
                    new ChannelAccount
                    {
                        Id = user.Id,
                    },
                },
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo
                    {
                        Id = tenantId,
                    },
                },
            };

            // In a production application, do not make this call until you are ready to send a message
            // this call will make the chat with this bot appear in the chat list & float it to the top w/ no messages. 
            var response = await connectorClient.Conversations.CreateConversationAsync(conversationParams);
            this.LogConversationCoordinates("User", serviceUrl, response.Id);
        }

        private async Task CreateConversationForChannelAsync(string tenantId, string channelId, string serviceUrl)
        {
            var connectorClient = this.connectorClientFactory.GetConnectorClient(serviceUrl);
            var activity = MessageFactory.Text($"New thread started by {nameof(CoordinateLoggerActivityHandler)}");
            var conversationParameters = new ConversationParameters
            {
                  IsGroup = true,
                  ChannelData = new TeamsChannelData
                  {
                      Channel = new ChannelInfo(channelId),
                  },
                  Activity = activity,
            };

            // In a production application, do not make this call until you are ready to send a message
            // this call will start a new thread inside the channel.
            var response = await connectorClient.Conversations.CreateConversationAsync(conversationParameters);

            this.LogConversationCoordinates("Channel", serviceUrl, response.Id);
        }

        private void LogConversationCoordinates(string type, string serviceUrl, string conversationId)
        {
            this.logger.LogInformation("{0} Conversation Coordinates: ({1}, {2})", type, serviceUrl, conversationId);
        }
    }
}