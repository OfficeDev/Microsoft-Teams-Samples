// <copyright file="ConversationService.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.PeoplePicker.Entities;

    /// <summary>
    /// Conversation service implementation.
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly ILogger<ConversationService> logger;
        private readonly IOptions<AppSettings> appSettingsOptions;
        private readonly GraphServiceClient graphServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationService"/> class.
        /// </summary>
        /// <param name="logger">logger.</param>
        /// <param name="appSettingsOptions">appSettingsOptions.</param>
        /// <param name="graphServiceClient">graphServiceClient.</param>
        public ConversationService(ILogger<ConversationService> logger, IOptions<AppSettings> appSettingsOptions, GraphServiceClient graphServiceClient)
        {
            this.appSettingsOptions = appSettingsOptions ?? throw new ArgumentNullException(nameof(appSettingsOptions));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        }

        /// <inheritdoc/>
        public async Task<string> CreateConversationAsync(ConversationContext conversationContext)
        {
            conversationContext = conversationContext ?? throw new ArgumentNullException(nameof(conversationContext));

            var members = new ChatMembersCollectionPage();
            foreach (var user in conversationContext.Users)
            {
                members.Add(new AadUserConversationMember
                {
                    Roles = new List<string>()
                    {
                        "owner",
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "user@odata.bind", string.Format("{0}/users('{1}')", this.appSettingsOptions.Value.BaseUrl, user) },
                    },
                });
            }

            var chat = new Chat
            {
                ChatType = ChatType.Group,
                Topic = conversationContext.Title,
                Members = members,
            };

            try
            {
                var chatResponse = await this.graphServiceClient.Chats
                .Request()
                .AddAsync(chat);

                return chatResponse?.Id;
            }
            catch (Exception exception)
            {
                this.logger.LogInformation("Exception while creating conversation {0}", exception);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> AddAppToConversationAsync(ConversationContext conversationContext)
        {
            conversationContext = conversationContext ?? throw new ArgumentNullException(nameof(conversationContext));

            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", string.Format("{0}/appCatalogs/teamsApps/{1}", this.appSettingsOptions.Value.BaseUrl, this.appSettingsOptions.Value.CatalogAppId) },
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

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SendProactiveMessageAsync(ITurnContext<IInvokeActivity> turnContext, ConversationContext conversationContext)
        {
            conversationContext = conversationContext ?? throw new ArgumentNullException(nameof(conversationContext));

            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            var conversationReference = new ConversationReference
            {
                ServiceUrl = turnContext.Activity.ServiceUrl ?? throw new ArgumentNullException(nameof(turnContext.Activity.ServiceUrl)),
                Conversation = new ConversationAccount
                {
                    Id = conversationContext.ConversationId ?? throw new ArgumentNullException(nameof(conversationContext.ConversationId)),
                },
            };

            await turnContext.Adapter.ContinueConversationAsync(
                this.appSettingsOptions.Value.ClientId,
                conversationReference,
                async (context, token) =>
                {
                    await context.SendActivityAsync(conversationContext.Message);
                },
                default(CancellationToken));
        }
    }
}
