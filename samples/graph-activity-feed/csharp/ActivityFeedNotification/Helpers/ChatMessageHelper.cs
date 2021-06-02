using AdaptiveCards;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabActivityFeed.Model;

namespace TabActivityFeed.Helpers
{
    public class ChatMessageHelper
    {
        public readonly IConfiguration _configuration;
        IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();

        public ChatMessageHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task<ChatMessage> CreateChatMessageForChannel(TaskInfo taskInfo)
        {
            GraphServiceClient graphClientChat = GetGraphClient();
            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskInfo.DeployementTitle
                },
            };
            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            var channelMessage = await graphClientChat.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return channelMessage;
        }

        public async Task<ChatMessage> CreateChannelMessageAdaptiveCard(TaskInfo taskInfo)
        {
            GraphServiceClient graphClientChat = GetGraphClient();
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                          {
                         new AdaptiveTextBlock()
                              {
                                  Text="Here is Your Reservation Details:",
                                  Weight = AdaptiveTextWeight.Bolder,
                                  Size = AdaptiveTextSize.Large,
                                  Id="taskDetails"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskInfo.reservationId,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskTitle"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskInfo.DeployementTitle,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskdesc"
                              },
                               new AdaptiveTextBlock()
                              {
                                  Text=taskInfo.currentSlot,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskslot"
                               }
                          }

            };


            var chatMessage = new ChatMessage
            {
                Subject = "Reservation Activtity:",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "<attachment id=\"74d20c7f34aa4a7fb74e2b30004247c5\"></attachment>"
                },
                Attachments = new List<ChatMessageAttachment>()
                              {
                                  new ChatMessageAttachment
                                    {
                                          Id = "74d20c7f34aa4a7fb74e2b30004247c5",
                                          ContentType = "application/vnd.microsoft.card.adaptive",
                                          ContentUrl = null,
                                          Content =  JsonConvert.SerializeObject(Card),
                                          Name = null,
                                          ThumbnailUrl = null
                                }
                           }
            };

            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            var getChannelMessage = await graphClientChat.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChannelMessage;

        }

        public  async Task<ChatMessage> CreatePendingFinanceRequestCard(TaskInfo taskInfo)
        {
            GraphServiceClient graphClientChat = GetGraphClient();
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                          {
                         new AdaptiveTextBlock()
                              {
                                  Text="Here is Your Task Details in Teams",
                                  Weight = AdaptiveTextWeight.Bolder,
                                  Size = AdaptiveTextSize.Large,
                                  Id="taskDetails"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskInfo.title,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskTitle"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskInfo.description,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskdesc"
                              },
                          }

            };


            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "<attachment id=\"74d20c7f34aa4a7fb74e2b30004247c5\"></attachment>"
                },
                Attachments = new List<ChatMessageAttachment>()
                              {
                                  new ChatMessageAttachment
                                    {
                                          Id = "74d20c7f34aa4a7fb74e2b30004247c5",
                                          ContentType = "application/vnd.microsoft.card.adaptive",
                                          ContentUrl = null,
                                          Content =  JsonConvert.SerializeObject(Card),
                                          Name = null,
                                          ThumbnailUrl = null
                                }
                           }
            };

            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            var getChannelMessage = await graphClientChat.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChannelMessage;
        }

        public async Task<ChatMessage> CreateGroupChatMessage(TaskInfo taskInfo)
        {
            var graphClientChat = GetGraphClient();
            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskInfo.DeployementTitle
                },
            };
            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            var getChatMessage = await graphClientChat.Chats[taskInfo.chatId].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChatMessage;
        }

        public GraphServiceClient GetGraphClient()
        {
            string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
            IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                .Create(_configuration["MicrosoftAppId"])
                .WithTenantId(_configuration["tenantId"])
                .Build();
            UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
            GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
            string password = "<<Your Password>>";
            System.Security.SecureString passWordSecureString = new System.Security.SecureString();
            foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
            return graphClientChat;
        }
    }
}
