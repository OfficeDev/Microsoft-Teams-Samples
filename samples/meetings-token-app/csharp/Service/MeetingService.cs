// <copyright file="MeetingService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using TokenApp.Repository;

    /// <summary>
    /// User role service.
    /// </summary>
    public class MeetingService : IMeetingService
    {
        private readonly HttpClient httpClient;
        private readonly AppCredentials botCredentials;
        private readonly IBotFrameworkHttpAdapter botFrameworkHttpAdapter;
        private readonly ITenantInfoRepository tenantInfoRepository;
        private readonly string teamsAppId;
        private readonly string contentBubbleUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The Http client factory.</param>
        /// <param name="botCredentials">The Bot credentials.</param>
        /// <param name="botAdapter">The Bot Framework HTTP adapter.</param>
        /// <param name="tenantInfoRepository">The TenantInfo repository.</param>
        /// <param name="meetingServiceOptions">App options related to the meetings service.</param>
        public MeetingService(
            IHttpClientFactory httpClientFactory,
            AppCredentials botCredentials,
            IBotFrameworkHttpAdapter botAdapter,
            ITenantInfoRepository tenantInfoRepository,
            IOptions<MeetingServiceOptions> meetingServiceOptions)
        {
            this.botCredentials = botCredentials;
            this.botFrameworkHttpAdapter = botAdapter;
            this.httpClient = httpClientFactory.CreateClient();
            this.tenantInfoRepository = tenantInfoRepository;
            this.teamsAppId = meetingServiceOptions.Value.TeamsAppId;
            this.contentBubbleUrl = meetingServiceOptions.Value.ContentBubbleUrl;
        }

        /// <inheritdoc/>
        public async Task<UserMeetingRoleServiceResponse> GetMeetingRoleAsync(string meetingId, string userId, string tenantId)
        {
            string serviceUri;
            var getServiceURL = this.tenantInfoRepository.GetServiceUrl(tenantId);

            if (getServiceURL != null)
            {
                serviceUri = getServiceURL;
            }
            else
            {
                serviceUri = "https://smba.trafficmanager.net/amer/";
            }

            using var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(serviceUri), string.Format("v1/meetings/{0}/participants/{1}?tenantId={2}", meetingId, userId, tenantId)));
            getRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await this.botCredentials.GetTokenAsync());

            using var getRoleResponse = await this.httpClient.SendAsync(getRoleRequest);
            getRoleResponse.EnsureSuccessStatusCode();

            var userRole = await getRoleResponse.Content.ReadAsStringAsync();
            var response = new UserMeetingRoleServiceResponse
            {
                UserRole = JsonConvert.DeserializeObject<Models.UserRole>(userRole),
            };
            
            return response;
        }

        /// <inheritdoc />
        public async Task PostStatusChangeNotification(
            string conversationId,
            string tenantId,
            int currentToken,
            string currentUserName)
        {
            // Create the activity to post to the meeting chat to trigger a notification
            // NOTE: The notification cannot be triggered by itself. To show the notification you must post a message to the chat.
            var contentBubbleUrlWithParam = new UriBuilder(this.contentBubbleUrl)
            {
                Query = Uri.EscapeDataString($"Token={currentToken}&User={currentUserName}"),
            };
            
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = string.Format("Current Token: {0}\nParticipant Name: {1}", currentToken, currentUserName),
                ChannelData = new ChannelData
                {
                    Notification = new Notification
                    {
                        // AlertInMeeting = true and the ExternalResourceUrl in this format cause a content bubble notification to be sent
                        AlertInMeeting = true,
                        ExternalResourceUrl = $"https://teams.microsoft.com/l/bubble/{this.teamsAppId}?url={contentBubbleUrlWithParam}&height=180&width=280&title=Token Update",
                    },
                },
            };

            // Build the conversation reference to get a turn context for the given conversation ID
            var conversationReference = new ConversationReference
            {
                ServiceUrl = this.tenantInfoRepository.GetServiceUrl(tenantId),
                ChannelId = "msteams",
                Conversation = new ConversationAccount
                {
                    Id = conversationId,
                },
            };

            // Continue the conversation to get a turn context for the conversation then post the activity
            await ((BotAdapter)this.botFrameworkHttpAdapter).ContinueConversationAsync(
                this.botCredentials.MicrosoftAppId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(activity);
                },
                CancellationToken.None);
        }
    }
}
