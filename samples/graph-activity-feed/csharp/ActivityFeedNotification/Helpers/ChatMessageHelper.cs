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
        public static string channelId;
        IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();

        public ChatMessageHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task<ChatMessage> CreateChatMessageForChannel(TaskDetails taskDetails, string accessToken)
        {
            GraphServiceClient graphClientChat = SimpleGraphClient.GetGraphClient(accessToken);
            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskDetails.DeployementTitle
                },
            };
            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            try
            {
                var channelMessage = await graphClientChat.Teams[taskDetails.teamId].Channels[taskDetails.channelId].Messages
                     .Request()
                     .AddAsync(chatMessage);
                return channelMessage;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public async Task<ChatMessage> CreateChannelMessageAdaptiveCard(TaskDetails taskDetails, string accessToken)
        {
            GraphServiceClient graphClientChat = SimpleGraphClient.GetGraphClient(accessToken);
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
                                  Text=taskDetails.reservationId,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskTitle"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskDetails.DeployementTitle,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskdesc"
                              },
                               new AdaptiveTextBlock()
                              {
                                  Text=taskDetails.currentSlot,
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
            var getChannelMessage = await graphClientChat.Teams[taskDetails.teamId].Channels[taskDetails.channelId].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChannelMessage;

        }

        public  async Task<ChatMessage> CreatePendingFinanceRequestCard(TaskDetails taskDetails, string accessToken)
        {
            GraphServiceClient graphClientChat= SimpleGraphClient.GetGraphClient(accessToken);
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
                                  Text=taskDetails.title,
                                  Weight = AdaptiveTextWeight.Lighter,
                                  Size = AdaptiveTextSize.Medium,
                                  Id="taskTitle"
                              },
                              new AdaptiveTextBlock()
                              {
                                  Text=taskDetails.description,
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
            var getChannelMessage = await graphClientChat.Teams[taskDetails.teamId].Channels[taskDetails.channelId].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChannelMessage;
        }

        public async Task<ChatMessage> CreateGroupChatMessage(TaskDetails taskDetails, string accessToken)
        {
            var graphClientChat = SimpleGraphClient.GetGraphClient(accessToken);
            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskDetails.DeployementTitle
                },
            };
            chatMessage.HostedContents = chatMessageHostedContentsCollectionPage;
            var getChatMessage = await graphClientChat.Chats[taskDetails.chatId].Messages
                 .Request()
                 .AddAsync(chatMessage);
            return getChatMessage;
        }
    }
}