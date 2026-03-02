using AdaptiveCards;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
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

        public ChatMessageHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task<ChatMessage> CreateChatMessageForChannel(TaskDetails taskDetails, string accessToken)
        {
            var graphClientChat = SimpleGraphClient.GetAuthenticatedClient(accessToken);

            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskDetails.DeployementTitle
                },
                HostedContents = new List<ChatMessageHostedContent>()
            };

            try
            {
                var channels = await graphClientChat.Teams[taskDetails.teamId].Channels.GetAsync();
                var defaultChannel = channels.Value.FirstOrDefault(c =>
                    c.DisplayName.Equals("General", StringComparison.OrdinalIgnoreCase));

                if (defaultChannel != null)
                {
                    var createdMessage = await graphClientChat.Teams[taskDetails.teamId]
                        .Channels[defaultChannel.Id].Messages
                        .PostAsync(chatMessage);

                    return createdMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding message: {ex.Message}");
            }

            return null;
        }

        public async Task<ChatMessage> CreateChannelMessageAdaptiveCard(TaskDetails taskDetails, string accessToken)
        {
            var graphClientChat = SimpleGraphClient.GetAuthenticatedClient(accessToken);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Here is Your Reservation Details:",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Id = "taskDetails"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = taskDetails.reservationId,
                        Weight = AdaptiveTextWeight.Lighter,
                        Size = AdaptiveTextSize.Medium,
                        Id = "taskTitle"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = taskDetails.DeployementTitle,
                        Weight = AdaptiveTextWeight.Lighter,
                        Size = AdaptiveTextSize.Medium,
                        Id = "taskdesc"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = taskDetails.currentSlot,
                        Weight = AdaptiveTextWeight.Lighter,
                        Size = AdaptiveTextSize.Medium,
                        Id = "taskslot"
                    }
                }
            };

            var chatMessage = new ChatMessage
            {
                Subject = "Reservation Activity:",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "<attachment id=\"74d20c7f34aa4a7fb74e2b30004247c5\"></attachment>"
                },
                Attachments = new List<ChatMessageAttachment>
                {
                    new ChatMessageAttachment
                    {
                        Id = "74d20c7f34aa4a7fb74e2b30004247c5",
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        ContentUrl = null,
                        Content = JsonConvert.SerializeObject(card),
                        Name = null,
                        ThumbnailUrl = null
                    }
                },
                HostedContents = new List<ChatMessageHostedContent>() 
            };

            try
            {
                var channels = await graphClientChat.Teams[taskDetails.teamId].Channels.GetAsync();
                var defaultChannel = channels.Value.FirstOrDefault(c =>
                    c.DisplayName.Equals("General", StringComparison.OrdinalIgnoreCase));

                if (defaultChannel != null)
                {
                    var createdMessage = await graphClientChat.Teams[taskDetails.teamId]
                        .Channels[defaultChannel.Id].Messages
                        .PostAsync(chatMessage);

                    return createdMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding message: {ex.Message}");
            }

            return null;
        }

        public async Task<ChatMessage> CreatePendingFinanceRequestCard(TaskDetails taskDetails, string accessToken)
        {
            var graphClientChat = SimpleGraphClient.GetAuthenticatedClient(accessToken);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Here is Your Task Details in Teams",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large,
                        Id = "taskDetails"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = taskDetails.title,
                        Weight = AdaptiveTextWeight.Lighter,
                        Size = AdaptiveTextSize.Medium,
                        Id = "taskTitle"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = taskDetails.description,
                        Weight = AdaptiveTextWeight.Lighter,
                        Size = AdaptiveTextSize.Medium,
                        Id = "taskdesc"
                    }
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
                Attachments = new List<ChatMessageAttachment>
                {
                    new ChatMessageAttachment
                    {
                        Id = "74d20c7f34aa4a7fb74e2b30004247c5",
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        ContentUrl = null,
                        Content = JsonConvert.SerializeObject(card),
                        Name = null,
                        ThumbnailUrl = null
                    }
                },
                HostedContents = new List<ChatMessageHostedContent>() // Updated
            };

            try
            {
                var channels = await graphClientChat.Teams[taskDetails.teamId].Channels.GetAsync();
                var defaultChannel = channels.Value.FirstOrDefault(c =>
                    c.DisplayName.Equals("General", StringComparison.OrdinalIgnoreCase));

                if (defaultChannel != null)
                {
                    var createdMessage = await graphClientChat.Teams[taskDetails.teamId]
                        .Channels[defaultChannel.Id].Messages
                        .PostAsync(chatMessage);

                    return createdMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding message: {ex.Message}");
            }

            return null;
        }

        public async Task<ChatMessage> CreateGroupChatMessage(TaskDetails taskDetails, string accessToken)
        {
            var graphClientChat = SimpleGraphClient.GetAuthenticatedClient(accessToken);

            var chatMessage = new ChatMessage
            {
                Subject = null,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = "New Deployment: " + taskDetails.DeployementTitle
                },
                HostedContents = new List<ChatMessageHostedContent>()
            };

            var getChatMessage = await graphClientChat.Chats[taskDetails.chatId].Messages
                .PostAsync(chatMessage);

            return getChatMessage;
        }
    }
}
