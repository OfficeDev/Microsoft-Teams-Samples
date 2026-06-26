// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using MessagingExtensionReminder.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace MessagingExtensionReminder.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        private readonly BotState _conversationState;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, ConcurrentDictionary<string, ConversationReference> conversationReferences, ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _taskDetails = taskDetails;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var title = string.Empty;
            var description = string.Empty;

            if (action.MessagePayload.Subject != null && turnContext.Activity.Conversation.ConversationType != "personal" && action.MessagePayload.Subject != "")
            {
                description = action.MessagePayload.Body.Content;
                title = action.MessagePayload.Subject;
            }
            else
            {
                title = action.MessagePayload.Body.Content;
            }

            return Task.FromResult(GetTaskModuleResponse(title, description));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null && turnContext.Activity.MembersAdded.Any(member => member.Id == turnContext.Activity.Recipient.Id))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can schedule a message reminder by selecting `...` over the message then select more action and then create-reminder and you will get reminder of the message at scheduled date and time."), cancellationToken);
            }
        }

        protected override async Task<MessagingExtensionActionResponse?> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            AddConversationReference((turnContext.Activity as Activity)!);
            var asJobject = JObject.FromObject(action.Data);
            var title = (string?)asJobject.ToObject<TaskDetails<string>>()?.Title ?? string.Empty;
            var description = (string?)asJobject.ToObject<TaskDetails<string>>()?.Description ?? string.Empty;
            var dateTime = (DateTime)(asJobject.ToObject<TaskDetails<DateTime>>()?.DateTime ?? DateTime.MinValue);

            var date = dateTime.ToLocalTime();

            _taskDetails.TryGetValue("taskDetails", out var currentTaskList);

            var taskDetails = new SaveTaskDetail()
            {
                Description = description,
                Title = title,
                DateTime = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, TimeSpan.Zero),
            };

            if (currentTaskList == null)
            {
                var taskList = new List<SaveTaskDetail> { taskDetails };
                _taskDetails.AddOrUpdate("taskDetails", taskList, (key, newValue) => taskList);
            }
            else
            {
                currentTaskList.Add(taskDetails);
                _taskDetails.AddOrUpdate("taskDetails", currentTaskList, (key, newValue) => currentTaskList);
            }

            var taskSchedule = new TaskScheduler();
            taskSchedule.Start(date.Year, date.Month, date.Day, date.Hour, date.Minute, _applicationBaseUrl);
            await turnContext.SendActivityAsync("Task submitted successfully. You will get reminder for the task at scheduled time", cancellationToken: cancellationToken);

            return null;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        private MessagingExtensionActionResponse GetTaskModuleResponse(string title, string description)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Url = _applicationBaseUrl + "/" + "ScheduleTask?title=" + title + "&description=" + description,
                        Height = 350,
                        Width = 400,
                        Title = "Schedule-task",
                    },
                },
            };
        }
    }
}