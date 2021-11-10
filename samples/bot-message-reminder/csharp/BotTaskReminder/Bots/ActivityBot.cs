// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BotTaskReminder.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BotTaskReminder.Bots
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

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, ConcurrentDictionary<string, ConversationReference> conversationReferences, ConcurrentDictionary<string, List<SaveTaskDetail>> taskDetails)
        {
            _conversationReferences = conversationReferences;
            _conversationState = conversationState;
            _taskDetails = taskDetails;
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var taskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out taskList);

            var attachmentlist = new List<MessagingExtensionAttachment>();
            foreach (var task in taskList)
            {
                var previewCard = new ThumbnailCard {
                    Title = task.Title,
                    Text = task.Description,
                    Tap = new CardAction { Type = "invoke", Value = task } 
                };
                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewCard,
                    Preview = previewCard.ToAttachment()
                };
                attachmentlist.Add(attachment);
            }

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachmentlist
                }
            };
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            var asObject = ((JObject)query).ToObject<SaveTaskDetail>();

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Task title:"+ asObject.Title,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true, 
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Task description:"+ asObject.Description,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true,
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
                            Id="schedule",
                            Title = asObject.Title,
                            Description = asObject.Description
                        },
                    }
                },
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
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
                    GetTaskList();
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample you can schedule a task and get reminder on the scheduled date and time.(use command 'create-reminder')."), cancellationToken);
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
            var title = (string)asJobject.ToObject<TaskDetails<string>>()?.Title;
            var description = (string)asJobject.ToObject<TaskDetails<string>>()?.Description;
            var taskModuleResponse = new TaskModuleResponse();

            if (buttonType == "schedule")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/" + "ScheduleTask?title="+title+"&description="+ description,
                        Height = 350,
                        Width = 350,
                        Title = "Schedule Task",
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
            AddConversationReference(turnContext.Activity as Activity);
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var title = (string)asJobject.ToObject<TaskDetails<string>>()?.Title;
            var description = (string)asJobject.ToObject<TaskDetails<string>>()?.Description;
            var dateTime = (string)asJobject.ToObject<TaskDetails<string>>()?.DateTime;

            var year = Convert.ToInt32(dateTime.Substring(0, 4));
            var month = Convert.ToInt32(dateTime.Substring(5, 2));
            var day = Convert.ToInt32(dateTime.Substring(8, 2));
            var hour = Convert.ToInt32(dateTime.Substring(11, 2));
            var min = Convert.ToInt32(dateTime.Substring(14, 2));

            List<SaveTaskDetail> taskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out taskList);

            var task = taskList.FirstOrDefault(task => task.Title == title);
            task.DateTime = new DateTimeOffset(year, month, day, hour, min, 0, TimeSpan.Zero);
            _taskDetails.AddOrUpdate("taskDetails", taskList, (key, newValue) => taskList);

            TaskScheduler taskSchedule = new TaskScheduler();
            taskSchedule.Start(year, month, day, hour, min, _applicationBaseUrl);
            await turnContext.SendActivityAsync("Task submitted successfully. You will get reminder for the task at scheduled time");

            return null;
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

        // Generate task list.
        private void GetTaskList()
        {
            var taskList = new List<SaveTaskDetail>(){
                new SaveTaskDetail()
                {
                    Title= "Scrum call",
                    Description = "Daily task update and blocker discussion with team"
                },
                new SaveTaskDetail()
                {
                    Title= "Testing",
                    Description = "Daily task update and blocker discussion with team"
                },
                new SaveTaskDetail()
                {
                    Title= "Development",
                    Description = "Daily task update and blocker discussion with team"
                },
                new SaveTaskDetail()
                {
                    Title= "SLA call",
                    Description = "Daily task update and blocker discussion with team"
                },
                new SaveTaskDetail()
                {
                    Title= "Client call",
                    Description = "Daily task update and blocker discussion with team"
                },
            };

            _taskDetails.AddOrUpdate("taskDetails", taskList, (key, newValue) => taskList);
        }
    }
}