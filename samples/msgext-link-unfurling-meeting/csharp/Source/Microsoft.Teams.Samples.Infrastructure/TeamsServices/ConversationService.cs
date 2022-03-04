// <copyright file="ConversationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;

    /// <summary>
    /// Conversation service implementation.
    /// </summary>
    internal class ConversationService : IConversationService
    {
        private readonly GraphServiceClient graphServiceClient;
        private readonly IAppSettings appSettings;
        private readonly ILogger<ConversationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <param name="appSettings">App settings.</param>
        /// <param name="logger">Logger.</param>
        public ConversationService(
            GraphServiceClient graphServiceClient,
            IAppSettings appSettings,
            ILogger<ConversationService> logger)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<string> AddTabToConversationAsync(ConversationContext conversationContext, TabInfo tabInfo)
        {
            // Check if a tab is already added.
            var existingTabs = await this.graphServiceClient.Chats[conversationContext.ConversationId].Tabs
                .Request().GetAsync();
            var resourceTab = existingTabs.Where(tab => tabInfo.EntityId.Equals(tab.Configuration.EntityId)).FirstOrDefault();
            if (resourceTab != null)
            {
                // Return an existing tab id.
                return resourceTab.Id;
            }

            // Add a new tab.
            var teamsTab = new TeamsTab
            {
                DisplayName = tabInfo.DisplayName,
                Configuration = new TeamsTabConfiguration
                {
                    EntityId = tabInfo.EntityId,
                    ContentUrl = tabInfo.ContentUrl,
                    WebsiteUrl = tabInfo.WebsiteUrl,
                    RemoveUrl = tabInfo.RemoveUrl,
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{this.appSettings.CatalogAppId}" },
                },
            };

            var result = await this.graphServiceClient.Chats[conversationContext.ConversationId].Tabs
                .Request()
                .AddAsync(teamsTab);

            return result?.Id;
        }

        /// <inheritdoc/>
        public async Task<string> AddApplicationToConversationAsync(ConversationContext conversationContext)
        {
            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{this.appSettings.CatalogAppId}" },
                },
            };

            try
            {
                var result = await this.graphServiceClient.Chats[conversationContext.ConversationId].InstalledApps
                    .Request()
                    .AddAsync(teamsAppInstallation);

                return result?.Id;
            }
            catch (ServiceException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Conflict)
                {
                    this.logger.LogInformation($"App is already installed. Inner error code: {exception.Error.Code}");
                    return string.Empty;
                }

                throw exception;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetJoinMeetingUrlForChatAsync(ConversationContext conversationContext, string userId)
        {
            var chat = await this.graphServiceClient
                .Users[userId]
                .Chats[conversationContext.ConversationId]
                .Request()
                .GetAsync();

            // Return join meeting url.
            return chat.OnlineMeetingInfo?.JoinWebUrl;
        }
    }
}
