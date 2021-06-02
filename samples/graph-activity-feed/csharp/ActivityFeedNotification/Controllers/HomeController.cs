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
using TabActivityFeed.Helpers;
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
            TaskHelper.AddTaskToFeed(taskInfo);
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
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskInfo);
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
            TaskHelper.AddTaskToFeed(taskInfo);
            var graphClient = SimpleGraphClient.GetGraphClient(_configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            if (taskInfo.taskInfoAction == "customTopic")
            {

                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChatMessage = chatMessage.CreateGroupChatMessage(taskInfo);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/chats/" + taskInfo.chatId + "/messages/" + getChatMessage.Id
                };

                var CustomActivityType = "approvalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "Deployment requires your approval"
                };
                var customRecipient = new AadUserNotificationRecipient
                {
                    UserId = user.Id
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
                    UserId = user.Id
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

            TaskHelper.AddTaskToFeed(taskInfo);
            var graphClient = SimpleGraphClient.GetGraphClient(_configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            if (taskInfo.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskInfo);
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
                    UserId = user.Id
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
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = chatMessage.CreateChannelMessageAdaptiveCard(taskInfo);

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
                    UserId = user.Id
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

                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreatePendingFinanceRequestCard(taskInfo);

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
                    UserId = user.Id
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
