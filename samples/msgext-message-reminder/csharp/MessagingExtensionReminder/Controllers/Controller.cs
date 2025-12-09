// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json;
using MessagingExtensionReminder.Helpers;
using MessagingExtensionReminder.Models;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;

namespace MessagingExtensionReminder.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;
        private readonly ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> _conversationReferences;
        private readonly string _applicationBaseUrl;

        public Controller(
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails,
            ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference> conversationReferences,
            IConfiguration configuration)
        {
            _taskDetails = taskDetails;
            _conversationReferences = conversationReferences;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new ArgumentNullException("ApplicationBaseUrl");
        }

        // Handles message extension fetch task and displays the schedule task form
        [MessageExtension.FetchTask]
        public object OnMessageExtensionFetchTask(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.FetchTaskActivity activity,
            [Context] IContext.Client client,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            StoreConversationReference(activity, log);
            var title = string.Empty;
            var description = string.Empty;
            if (activity.Value?.MessagePayload?.Subject != null &&
                !string.IsNullOrEmpty(activity.Value.MessagePayload.Subject))
            {
                description = activity.Value.MessagePayload?.Body?.Content ?? "";
                title = activity.Value.MessagePayload.Subject;
            }
            else
            {
                title = activity.Value?.MessagePayload?.Body?.Content ?? "";
            }
            var taskModuleUrl = $"{_applicationBaseUrl}/ScheduleTask?title={Uri.EscapeDataString(title)}&description={Uri.EscapeDataString(description)}";
            return new
            {
                task = new
                {
                    type = "continue",
                    value = new
                    {
                        url = taskModuleUrl,
                        height = 350,
                        width = 400,
                        title = "Schedule-task"
                    }
                }
            };
        }

        // Handles task submission, saves task details, and sends confirmation message
        [MessageExtension.SubmitAction]
        public object OnMessageExtensionSubmitAction(
            [Context] Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity,
            [Context] IContext.Client client,
            [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            StoreConversationReference(activity, log);
            var data = activity.Value?.Data;
            var jsonString = JsonSerializer.Serialize(data);
            var taskDetails = JsonSerializer.Deserialize<TaskDetails>(jsonString);
            if (taskDetails == null || taskDetails.Title == null || taskDetails.DateTime == null)
            {
                return MessageExtensionResponseHelper.CreateErrorActionResponse("Invalid task data");
            }
            var taskDateTime = new DateTimeOffset(
                taskDetails.DateTime.Value.Year,
                taskDetails.DateTime.Value.Month,
                taskDetails.DateTime.Value.Day,
                taskDetails.DateTime.Value.Hour,
                taskDetails.DateTime.Value.Minute,
                0,
                DateTimeOffset.Now.Offset);
            var currentTaskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out currentTaskList);
            var saveTaskDetail = new SaveTaskDetail
            {
                Description = taskDetails.Description,
                Title = taskDetails.Title,
                DateTime = taskDateTime
            };
            if (currentTaskList == null)
            {
                currentTaskList = new List<SaveTaskDetail> { saveTaskDetail };
                _taskDetails.AddOrUpdate("taskDetails", currentTaskList, (key, newValue) => currentTaskList);
            }
            else
            {
                currentTaskList.Add(saveTaskDetail);
                _taskDetails.AddOrUpdate("taskDetails", currentTaskList, (key, newValue) => currentTaskList);
            }
            var taskScheduler = new Helpers.TaskScheduler();
            taskScheduler.Start(
                taskDateTime.Year,
                taskDateTime.Month,
                taskDateTime.Day,
                taskDateTime.Hour,
                taskDateTime.Minute,
                _applicationBaseUrl);
            var confirmationMessage = "Task submitted successfully. You will get a reminder for the task at the scheduled time.";
            client.Send(confirmationMessage).Wait();
            return new { };
        }

        // Stores conversation reference for sending proactive messages
        private void StoreConversationReference(
            Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.FetchTaskActivity activity,
            Microsoft.Teams.Common.Logging.ILogger log)
        {
            var conversationReference = new Microsoft.Teams.Api.ConversationReference
            {
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                Conversation = activity.Conversation,
                Bot = activity.Recipient,
                User = activity.From
            };
            if (!string.IsNullOrEmpty(activity.From?.Id))
            {
                _conversationReferences.AddOrUpdate(
                    activity.From.Id,
                    conversationReference,
                    (key, oldValue) => conversationReference);
            }
        }

        // Stores conversation reference for sending proactive messages
        private void StoreConversationReference(
            Microsoft.Teams.Api.Activities.Invokes.MessageExtensions.SubmitActionActivity activity,
            Microsoft.Teams.Common.Logging.ILogger log)
        {
            var conversationReference = new Microsoft.Teams.Api.ConversationReference
            {
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                Conversation = activity.Conversation,
                Bot = activity.Recipient,
                User = activity.From
            };
            if (!string.IsNullOrEmpty(activity.From?.Id))
            {
                _conversationReferences.AddOrUpdate(
                    activity.From.Id,
                    conversationReference,
                    (key, oldValue) => conversationReference);
            }
        }
    }
}
