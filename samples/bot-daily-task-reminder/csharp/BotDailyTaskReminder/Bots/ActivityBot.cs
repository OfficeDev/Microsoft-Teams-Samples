// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    /// Handles incoming bot activities such as messages, task module fetch, and task module submission.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private readonly string _applicationBaseUrl;
        protected readonly BotState _conversationState;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly ConcurrentDictionary<string, List<SaveTaskDetail>> _taskDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityBot"/> class.
        /// </summary>
        public ActivityBot(IConfiguration configuration,
            ConversationState conversationState,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _taskDetails = taskDetails;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        /// <summary>
        /// Handles when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context of the message activity.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.ToLower().Trim() == "create-reminder")
            {
                // Adds the current conversation reference and sends the task scheduling adaptive card
                AddConversationReference(turnContext.Activity as Activity);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(GetAdaptiveCardForTaskModule()), cancellationToken);
            }
        }

        /// <summary>
        /// Handles the completion of a turn, saving any state changes.
        /// </summary>
        /// <param name="turnContext">The context of the current turn.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any changes made to conversation state during this turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when the bot is added to a conversation.
        /// Sends a welcome message to new members.
        /// </summary>
        /// <param name="membersAdded">The members added to the conversation.</param>
        /// <param name="turnContext">The context of the current turn.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous task.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Sends a greeting message when a new user is added
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! Use the command 'create-reminder' to schedule a recurring task and receive reminders."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles task module fetch requests.
        /// </summary>
        /// <param name="turnContext">The context of the turn.</param>
        /// <param name="taskModuleRequest">The request payload for the task module.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous task.</param>
        /// <returns>The task module response to send back.</returns>
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
                        Url = _applicationBaseUrl + "/ScheduleTask",
                        Height = 450,
                        Width = 450,
                        Title = "Schedule a task",
                    },
                };
            }

            return Task.FromResult(taskModuleResponse);
        }

        /// <summary>
        /// Handles task module submission requests.
        /// </summary>
        /// <param name="turnContext">The context of the turn.</param>
        /// <param name="taskModuleRequest">The request payload for the task module.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous task.</param>
        /// <returns>The task module response to send back.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var title = (string)asJobject.ToObject<TaskDetails<string>>()?.Title;
            var description = (string)asJobject.ToObject<TaskDetails<string>>()?.Description;
            var dateTime = (DateTime)asJobject.ToObject<TaskDetails<DateTime>>()?.DateTime;
            var selectedDaysObject = (JArray)asJobject.ToObject<TaskDetails<JArray>>()?.SelectedDays;
            var selectedDays = selectedDaysObject.ToObject<DayOfWeek[]>();
            var date = dateTime.ToLocalTime();

            // Prepare task details
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

            // Add the task to the task list
            if (currentTaskList == null)
            {
                taskList.Add(taskDetails);
                _taskDetails.AddOrUpdate("taskDetails", taskList, (key, newValue) => taskList);
            }
            else
            {
                currentTaskList.Add(taskDetails);
                _taskDetails.AddOrUpdate("taskDetails", currentTaskList, (key, newValue) => currentTaskList);
            }

            // Schedule the task
            TaskScheduler taskSchedule = new TaskScheduler();
            taskSchedule.Start(date.Hour, date.Minute, _applicationBaseUrl, selectedDays);

            // Send a success message to the user
            await turnContext.SendActivityAsync("Task submitted successfully, you will get a recurring reminder for the task at a scheduled time");

            return null;
        }

        /// <summary>
        /// Creates and returns the adaptive card for scheduling a task.
        /// </summary>
        /// <returns>The adaptive card as an attachment.</returns>
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
        /// Adds a conversation reference for the current activity.
        /// </summary>
        /// <param name="activity">The bot activity.</param>
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }
    }
}
