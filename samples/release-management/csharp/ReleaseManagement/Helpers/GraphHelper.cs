// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Helpers
{
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using ReleaseManagement.Models.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class GraphHelper
    {
        private readonly IOptions<AzureSettings> azureSettings;
        public GraphHelper(IOptions<AzureSettings> azureSettings)
        {
            this.azureSettings = azureSettings;
        }

        /// <summary>
        /// Get client to call graph API.
        /// </summary>
        /// <param name="token">Application token.</param>
        /// <returns>Graph client.</returns>
        public GraphServiceClient GetAuthenticatedClient(string token)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }
                )
            );

            return graphClient;
        }

        /// <summary>
        /// Gets application token.
        /// </summary>
        /// <returns>Application token.</returns>
        public async Task<string> GetToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(this.azureSettings.Value.MicrosoftAppId)
                                                    .WithClientSecret(this.azureSettings.Value.MicrosoftAppPassword)
                                                    .WithAuthority($"https://login.microsoftonline.com/{this.azureSettings.Value.MicrosoftAppTenantId}")
                                                    .WithRedirectUri("https://daemon")
                                                    .Build();
            // TeamsAppInstallation.ReadWriteForChat.All Chat.Create
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        /// <summary>
        /// Install application in user Group chat
        /// </summary>
        /// <param name="GroupId">Id of group.</param>
        public async Task AppinstallationforGroupAsync(string GroupId)
        {
            string access_Token = await GetToken();
            GraphServiceClient graphClient = GetAuthenticatedClient(access_Token);

            try
            {
                var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{this.azureSettings.Value.AppExternalId}"}
                    }
                };

                await graphClient.Chats[GroupId].InstalledApps
                    .Request()
                    .AddAsync(userScopeTeamsAppInstallation);
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Creates group chat.
        /// </summary>
        /// <param name="userMails">Members mail to be added in group chat.</param>
        /// <param name="groupTitle">Title of group chat.</param>
        /// <returns>Created chat details.</returns>
        public async Task<Chat> CreateGroupChatAsync(IEnumerable<string> userMails, string groupTitle)
        {
            string accessToken = await GetToken();
            GraphServiceClient graphClient = GetAuthenticatedClient(accessToken);

            try
            {
                var chatMembersCollectionPage = new ChatMembersCollectionPage();
                foreach (var mail in userMails.Distinct())
                {
                    chatMembersCollectionPage.Add((ConversationMember)new AadUserConversationMember
                    {
                        Roles = new List<String>()
                        {
                            "owner"
                        },
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"user@odata.bind", $"https://graph.microsoft.com/v1.0/users/{mail}"}
                        }
                    });
                }

                var chat = new Chat
                {
                    ChatType = ChatType.Group,
                    Topic = groupTitle,
                    Members = chatMembersCollectionPage
                };

                var createdChat = await graphClient.Chats
                    .Request()
                    .AddAsync(chat);

                return createdChat;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                Console.WriteLine(ex);
                return new Chat();
            }
        }
    }
}
