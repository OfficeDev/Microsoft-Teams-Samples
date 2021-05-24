// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models.Sharepoint;
using MeetingExtension_SP.Repositories;
using MessageExtension_SP.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace MessageExtension_SP.Helpers
{
    /// <summary>
    /// Common for channel and bot
    /// </summary>
    public class Common
    {
        /// <summary>
        /// Send channel data
        /// </summary>
        /// <param name="card"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task SendChannelData(Attachment card, IConfiguration configuration)
        {
            //Teams channel id in which to create the post.
            string teamsChannelId = configuration["ChannelId"];

            //The Bot Service Url needs to be dynamically fetched (and stored) from the Team. Recommendation is to capture the serviceUrl from the bot Payload and later re-use it to send proactive messages.
            string serviceUrl = configuration["BotServiceUrl"];

            //From the Bot Channel Registration
            string botClientID = configuration["MicrosoftAppId"];
            string botClientSecret = configuration["MicrosoftAppPassword"];

            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), new MicrosoftAppCredentials(botClientID, botClientSecret));
            var topLevelMessageActivity = MessageFactory.Attachment(card);
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData
                {
                    Channel = new ChannelInfo(teamsChannelId),
                },
                Activity = (Activity)topLevelMessageActivity
            };

           var conversationResource = await connectorClient.Conversations.CreateConversationAsync(conversationParameters);
            var replyMessage = Activity.CreateMessageActivity();
            replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());

            //get and store the user details in temp file
            var tempFilePath = @"Temp/ConversationFile.txt";
            System.IO.File.WriteAllText(tempFilePath,replyMessage.Conversation.Id.ToString() );

        }

        /// <summary>
        /// Get asset details
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<AssetData> GetAssetDetails(IConfiguration configuration)
        {
            SharePointRepository repository = new SharePointRepository(configuration);
            var data = await repository.GetAllItemsAsync<DocumentLibrary>(configuration["StagingFolder"]);
            string readFileFromTemp = System.IO.File.ReadAllText(@"Temp/TempFile.txt");
            string filename = Path.GetFileName(readFileFromTemp).Split('_')[1];

            var recentFile = data.ToList().Where(x => x.Name.ToLower().Contains(filename.ToLower())).FirstOrDefault();           

            string[] submitter= System.IO.File.ReadAllText(@"Temp/UserFile.txt").Split(',');
            var submitterDetails= await GetUserDetails(configuration, submitter[1]);
            string ownerId = await GetManagerId(configuration);
            var approverName = await GetUserDetails(configuration, ownerId);

            AssetData assetData = new AssetData();
            assetData.ApproverName = approverName.displayName;
            assetData.DateOfSubmission = recentFile.TimeLastModified;
            assetData.NameOfDocument = recentFile.Name;
            assetData.SubmittedBy = submitterDetails.displayName;
            assetData.SubitteTo = configuration["ApprovedFolder"];
            assetData.DocName = filename;
            assetData.url = configuration["BaseURL"] + recentFile.ServerRelativeUrl;
            assetData.userMRI = ownerId;
            assetData.userChat = "https://teams.microsoft.com/l/chat/0/0?users="+submitterDetails.mail;
            return assetData;
        }

        /// <summary>
        /// Get manager Id from graph api
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<string> GetManagerId(IConfiguration configuration)
        {
            string tenantId = configuration["TenantId"];
            string groupId= configuration["TeamId"];
            try
            {
                var token = await GetToken(tenantId, configuration);
                GraphServiceClient graphClient = GetAuthenticatedClient(token.ToString());

                var result = await graphClient.Groups[groupId].Owners
                 .Request()
                 .GetAsync();

                return result.FirstOrDefault().Id;
            }
            catch(Exception ex)
            {

            }

            return string.Empty;

        }

        /// <summary>
        /// Get user details from graph api
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<UserModel> GetUserDetails(IConfiguration configuration,string userId)
        {
            string tenantId = configuration["TenantId"];

            try
            {
                var token = await GetToken(tenantId, configuration);
                GraphServiceClient graphClient = GetAuthenticatedClient(token.ToString());

                var user = await graphClient.Users[userId]
                    .Request()
                    .GetAsync();

                var details = new UserModel
                {
                    displayName = user.DisplayName,
                    mail = user.Mail,
                };
               
                return details;
            }
            catch (Exception ex)
            {

            }

            return null;

        }

        /// <summary>
        /// Get token
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<string> GetToken(string tenantId,IConfiguration configuration)
        {

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(configuration["MicrosoftAppId"])
                                                  .WithClientSecret(configuration["MicrosoftAppPassword"])
                                                  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        /// <summary>
        /// Get Auth client for Graph calls
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static GraphServiceClient GetAuthenticatedClient(string token)
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
                    }));
            return graphClient;
        }
    }
}
