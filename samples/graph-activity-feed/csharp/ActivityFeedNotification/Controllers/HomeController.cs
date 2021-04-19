using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using AdaptiveCards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using TabActivityFeed.Model;
using TabActivityFeed.Repository;

namespace TabActivityFeed.Controllers
{

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("hello")]
        public ActionResult Hello()
        {
            TaskInfo idTask = new TaskInfo();
            idTask.taskId = Guid.NewGuid();
            ViewBag.taskId = idTask.taskId.ToString("N");

            return View("Index");
        }

        [Route("configure")]
        public ActionResult Configure()
        {
            return View("Configure");
        }

        [Route("groupchatnotification")]
        [HttpGet]
        public ActionResult GroupChatNotification()
        {
            return View();
        }
        [Route("details")]
        [HttpGet]
        [System.Web.Mvc.ChildActionOnly]
        public ActionResult Details()
        {
            return PartialView(FeedRepository.Tasks);
        }

        [Route("teamnotification")]
        public ActionResult TeamNotification()
        {
            return View();
        }

        [HttpPost]
        [Route("SendNotificationToUser")]
        public async Task<ActionResult> SendNotificationToUser(TaskInfo taskInfo)
        {
            FeedRepository.Tasks.Add(new TaskInfo
            {
                title = taskInfo.title,
                DeployementTitle = taskInfo.DeployementTitle,
                description = taskInfo.description,
                DeploymentDescription = taskInfo.DeploymentDescription
            });
            var graphClient = SimpleGraphClient.GetGraphClient(_configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            var installedApps = await graphClient.Users[user.Id].Teamwork.InstalledApps
                               .Request()
                               .Expand("teamsAppDefinition")
                               .GetAsync();
            var installationId = installedApps.Where(id => id.TeamsAppDefinition.DisplayName == "NotifyFeedApp").Select(x => x.Id);
            var userName = user.UserPrincipalName;

            if (taskInfo.taskInfoAction == "customTopic")
            {

                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();

                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password = "<<Your Password>>";
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();

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
                var getChannelMessage = await graphClientChat.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Messages
                     .Request()
                     .AddAsync(chatMessage);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    Value = "Deployment Approvals Channel",
                    WebUrl = getChannelMessage.WebUrl
                };

                var CustomActivityType = "deploymentApprovalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "New deployment requires your approval"
                };

                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "deploymentId",
                   Value ="6788662"
                  }
                };
                try
                {
                    await graphClient.Users[user.Id].Teamwork
                        .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters)
                        .Request()
                        .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                ViewBag.taskID = new Guid();
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/users/" + user.Id + "/teamwork/installedApps/" + installationId.ToList()[0]
                };

                var activityType = "taskCreated";

                var previewText = new ItemBody
                {
                    Content = "New Task Created"
                };

                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {

            new Microsoft.Graph.KeyValuePair
            {
              Name = "taskName",
              Value =taskInfo.title
             }

           };
                try
                {
                    await graphClient.Users[user.Id].Teamwork
                        .SendActivityNotification(topic, activityType, null, previewText, templateParameters)
                        .Request()
                        .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return View("Index");
        }

        [HttpPost]
        [Route("SendNotificationToGroupChat")]
        public async Task<ActionResult> SendNotificationToGroupChat(TaskInfo taskInfo)
        {
            FeedRepository.Tasks.Add(new TaskInfo
            {
                title = taskInfo.title,
                DeployementTitle = taskInfo.DeployementTitle,
                description = taskInfo.description,
                DeploymentDescription = taskInfo.DeploymentDescription
            });
            var graphClient = SimpleGraphClient.GetGraphClient(_configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            if (taskInfo.taskInfoAction == "customTopic")
            {

                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();
                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password = "<<Your Password>>";
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();

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
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/chats/" + getChatMessage.ChatId + "/messages/" + getChatMessage.Id
                    //  WebUrl = 
                };

                var CustomActivityType = "approvalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "Deployment requires your approval"
                };
                var customRecipient = new AadUserNotificationRecipient
                {
                    UserId = "b130c271-d2eb-45f9-83ab-9eb3fe3788bc"
                };
                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "approvalTaskId",
                   Value ="2020AAGGTAPP"
                  }
                };
                try
                {
                    await graphClient.Chats[taskInfo.chatId]
                          .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters, customRecipient)
                          .Request()
                          .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();
                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password ="<<Your Password>>";
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();
                var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
                {
                    Body = new List<AdaptiveElement>()
                          {
                         new AdaptiveTextBlock()
                              {
                                  Text="Here is Your Task Details",
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
                var getChatMessage = await graphClientChat.Chats[taskInfo.chatId].Messages
                     .Request()
                     .AddAsync(chatMessage);
                var chatMembers = graphClientChat.Chats[taskInfo.chatId].Members
                    .Request()
                    .GetAsync().Result;
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/chats/" + taskInfo.chatId
                };

                var activityType = "taskCreated";

                var previewText = new ItemBody
                {
                    Content = "Hello:"
                };
                var recipient = new AadUserNotificationRecipient
                {
                    UserId = "b130c271-d2eb-45f9-83ab-9eb3fe3788bc"
                };


                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {

            new Microsoft.Graph.KeyValuePair
            {
              Name = "taskName",
              Value =taskInfo.title
             }

           };
                try
                {
                    await graphClient.Chats[taskInfo.chatId]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            return View("groupchatnotification");
        }
        [HttpPost]
        [Route("sendNotificationToTeam")]
        public async Task<ActionResult> sendNotificationToTeam(TaskInfo taskInfo)
        {

            FeedRepository.Tasks.Add(new TaskInfo
            {
                title = taskInfo.title,
                DeployementTitle = taskInfo.DeployementTitle,
                description = taskInfo.description,
                DeploymentDescription = taskInfo.DeploymentDescription
            });
            var graphClient = SimpleGraphClient.GetGraphClient(_configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            if (taskInfo.taskInfoAction == "customTopic")
            {

                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();
                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password = "<<Your Password>>"];
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();

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
                var getChannelMessage = await graphClientChat.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Messages
                     .Request()
                     .AddAsync(chatMessage);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    Value = "Deployment Approvals Channel",
                    WebUrl = getChannelMessage.WebUrl
                };

                var CustomActivityType = "approvalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "New deployment requires your approval"
                };
                var customRecipient = new AadUserNotificationRecipient
                {
                    UserId = "b130c271-d2eb-45f9-83ab-9eb3fe3788bc"
                };
                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "approvalTaskId",
                   Value ="5654653"
                  }
                };
                try
                {
                    await graphClient.Teams[_configuration["teamId"]]
                          .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters, customRecipient)
                          .Request()
                          .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (taskInfo.taskInfoAction == "channelTab")
            {
                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();
                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password = "<<Your Password>>";
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();
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

                var tabs = await graphClient.Teams[_configuration["teamId"]].Channels[_configuration["channelId"]].Tabs
                .Request()
    .Expand("teamsApp")
    .GetAsync();
                var tabId = tabs.Where(a => a.DisplayName == "NotifyFeedApp").Select(x => x.Id).ToArray()[0];
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/teams/" + _configuration["teamId"] + "/channels/" + _configuration["channelId"] + "/tabs/" + tabId
                };

                var activityType = "reservationUpdated";

                var previewText = new ItemBody
                {
                    Content = "Your Reservation Updated:"
                };
                var recipient = new AadUserNotificationRecipient
                {
                    UserId = "b130c271-d2eb-45f9-83ab-9eb3fe3788bc"
                };


                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {

            new Microsoft.Graph.KeyValuePair
            {
              Name = "reservationId",
              Value =taskInfo.reservationId
             },
              new Microsoft.Graph.KeyValuePair
            {
              Name = "currentSlot",
              Value =taskInfo.currentSlot
             }
           };
                try
                {
                    await graphClient.Teams[_configuration["TeamId"]]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else
            {
                IChatMessageHostedContentsCollectionPage chatMessageHostedContentsCollectionPage = new ChatMessageHostedContentsCollectionPage();
                string[] scopes = { "ChannelMessage.Send", "Group.ReadWrite.All", "User.Read" };
                IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                    .Create(_configuration["clientAppId"])
                    .WithTenantId(_configuration["tenantId"])
                    .Build();
                UsernamePasswordProvider authenticationProvider = new UsernamePasswordProvider(publicClientApplication, scopes);
                GraphServiceClient graphClientChat = new GraphServiceClient(authenticationProvider);
                string password = "<<Your Password>>";
                System.Security.SecureString passWordSecureString = new System.Security.SecureString();
                foreach (char c in password.ToCharArray()) passWordSecureString.AppendChar(c);
                User me = await graphClientChat.Me.Request()
                             .WithUsernamePassword("<<Your Username>>", passWordSecureString)
                             .GetAsync();
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
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    WebUrl = getChannelMessage.WebUrl,
                    Value = "Deep Link to Chat"
                };

                var activityType = "pendingFinanceApprovalRequests";

                var previewText = new ItemBody
                {
                    Content = "These are the count of pending request pending request:"
                };
                var recipient = new AadUserNotificationRecipient
                {
                    UserId = "b130c271-d2eb-45f9-83ab-9eb3fe3788bc"
                };


                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {

            new Microsoft.Graph.KeyValuePair
            {
              Name = "pendingRequestCount",
              Value ="5"
             }

           };
                try
                {
                    await graphClient.Teams[_configuration["TeamId"]]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            return View("teamnotification");
        }
    }


}
