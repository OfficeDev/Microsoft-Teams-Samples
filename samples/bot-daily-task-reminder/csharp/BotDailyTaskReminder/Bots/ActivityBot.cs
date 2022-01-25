// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BotDailyTaskReminder.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BotDailyTaskReminder.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        protected readonly BotState _conversationState;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;

        public ActivityBot(IConfiguration configuration, 
            ConversationState conversationState, 
            ConcurrentDictionary<string, 
            ConversationReference> conversationReferences, 
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _taskDetails = taskDetails;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.ToLower().Trim() == "create-reminder")
            {
                AddConversationReference(turnContext.Activity as Activity);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
            }           
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample you can schedule a recurring task and get reminder on the scheduled time.(use command 'create-reminder')."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handle task module is fetch.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var buttonType = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.Id;
            var taskModuleResponse = new TaskModuleResponse();

            if (buttonType == "schedule")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/" + "ScheduleTask",
                        Height = 450,
                        Width = 450,
                        Title = "Schedule a task",
                    },
                };
            }

            return Task.FromResult(taskModuleResponse);
        }

        /// <summary>
        /// Handle task module is submit.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var title = (string)asJobject.ToObject<TaskDetails<string>>()?.Title;
            var description = (string)asJobject.ToObject<TaskDetails<string>>()?.Description;
            var dateTime = (DateTime)asJobject.ToObject<TaskDetails<DateTime>>()?.DateTime;
            var selectedDaysObject = (JArray)asJobject.ToObject<TaskDetails<JArray>>()?.SelectedDays;
            var selectedDays = selectedDaysObject.ToObject<DayOfWeek[]>();
            var date = dateTime.ToLocalTime();

            var recurringDays = string.Join(",", selectedDays);
            var currentTaskList = new List<SaveTaskDetail>();
            List<SaveTaskDetail> taskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out currentTaskList);

            var taskDetails = new SaveTaskDetail()
            {
                Description = description,
                Title = title,
                DateTime = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, TimeSpan.Zero),
                SelectedDays = selectedDays
            };

            if(currentTaskList == null)
            {
                taskList.Add(taskDetails);
                _taskDetails.AddOrUpdate("taskDetails", taskList, (key, newValue) => taskList);
            }
            else
            {
                currentTaskList.Add(taskDetails);
                _taskDetails.AddOrUpdate("taskDetails", currentTaskList, (key, newValue) => currentTaskList);
            }
            
            TaskScheduler taskSchedule = new TaskScheduler();
            taskSchedule.Start(date.Hour, date.Minute, _applicationBaseUrl, selectedDays);
            await turnContext.SendActivityAsync("Task submitted successfully, you will get a recurring reminder for the task at a scheduled time");

            return null;
        }

        /// <summary>
        /// Sample Adaptive card for schedule task in button.
        /// </summary>
        private Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Please click here to schedule a recurring task reminder",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Schedule task",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id = "schedule"
                        },
                    }
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Method to add conversation reference.
        /// </summary>
        /// <param name="activity">Bot activity</param>
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }
    }
}