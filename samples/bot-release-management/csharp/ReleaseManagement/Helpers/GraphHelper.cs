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
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class GraphHelper
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
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

            // TeamsAppInstallation.ReadWriteForChat.All Chat.Create User.Read.All TeamsAppInstallation.ReadWriteForChat.All
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
                var appInternalId = await GetApplicationInternalId(graphClient);
                var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appInternalId}"}
                    }
                };

                await graphClient.Chats[GroupId].InstalledApps
                    .Request()
                    .AddAsync(userScopeTeamsAppInstallation);
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
               throw ex;
            }
        }

        /// <summary>
        /// Creates group chat.
        /// </summary>
        /// <param name="userMails">Members mail to be added in group chat.</param>
        /// <param name="groupTitle">Title of group chat.</param>
        /// <returns>Created chat details.</returns>
        public async Task<string> CreateGroupChatAsync(IEnumerable<string> userMails, string groupTitle)
        {
            string accessToken = await GetToken();
            GraphServiceClient graphClient = GetAuthenticatedClient(accessToken);
            var retry = 1;
            do
            {
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

                    return (await graphClient.Chats
                        .Request()
                        .AddAsync(chat)).Id;
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    retry++;
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
            while (retry < 6);
            return "";
        }

        /// <summary>
        /// Gets the user profile by user principal name.
        /// </summary>
        /// <param name="userPrincipalName">User principal name</param>
        /// <returns>User profile pictrue in base64 format.</returns>
        public async Task<string> GetProfilePictureByUserPrincipalNameAsync (string userPrincipalName)
        {
            string accessToken = await GetToken();
            GraphServiceClient graphClient = GetAuthenticatedClient(accessToken);
            try
            {
                var userInfo = await graphClient.Users[userPrincipalName]
                .Request()
                .GetAsync();

                var stream = await graphClient.Users[userInfo.Id].Photo.Content
                .Request()
                .GetAsync();

                var picture = this.ReadFully(stream);
                var base64String = Convert.ToBase64String(picture);
                var imageUrl = "data:image/png;base64," + base64String;

                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Converts binary stream to byte array.
        /// </summary>
        /// <param name="input">input stream.</param>
        /// <returns>byte array.</returns>
        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[input.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get application internal Id with external Id
        /// </summary>
        /// <param name="graphClient">Graph client to make graph API calls.</param>
        /// <returns>Internal app Id.</returns>
        private async Task<string> GetApplicationInternalId(GraphServiceClient graphClient)
        {
            try
            {
                return (await graphClient.AppCatalogs.TeamsApps
                    .Request()
                    .Filter($"externalId eq '{this.azureSettings.Value.MicrosoftAppId}'")
                    .GetAsync()).FirstOrDefault().Id;
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                throw ex;
            }
        }
    }
}
