// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using METaskReminder.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace METaskReminder.Bots
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

        /// <summary>
        /// When OnTurn method receives a submit invoke activity on bot turn, it calls this method.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Provides context for a turn of a bot and.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var title = action.MessagePayload.Body.Content.ToString();

            return this.GetTaskModuleResponse(title);
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
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! With this sample you can schedule a task from messaging extension and get reminder on the scheduled date and time."), cancellationToken);
                }
            }
        }

        /// <summary>
        ///  Handle message extension submit action task received by the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Messaging extension action value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Response of messaging extension action.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionfetchtaskasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            var asJobject = JObject.FromObject(action.Data);
            var title = (string)asJobject.ToObject<TaskDetails<string>>()?.Title;
            var description = (string)asJobject.ToObject<TaskDetails<string>>()?.Description;
            var dateTime = (string)asJobject.ToObject<TaskDetails<string>>()?.DateTime;

            var year = Convert.ToInt32(dateTime.Substring(0, 4));
            var month = Convert.ToInt32(dateTime.Substring(5, 2));
            var day = Convert.ToInt32(dateTime.Substring(8, 2));
            var hour = Convert.ToInt32(dateTime.Substring(11, 2));
            var min = Convert.ToInt32(dateTime.Substring(14, 2));

            var currentTaskList = new List<SaveTaskDetail>();
            List<SaveTaskDetail> taskList = new List<SaveTaskDetail>();
            _taskDetails.TryGetValue("taskDetails", out currentTaskList);

            var taskDetails = new SaveTaskDetail()
            {
                Description = description,
                Title = title,
                DateTime = new DateTimeOffset(year, month, day, hour, min, 0, TimeSpan.Zero),
            };

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

        /// <summary>
        /// Get messaging extension action response object to show collection of question answers.
        /// </summary>
        /// <param name="questionAnswerCard">Question answer card as input.</param>
        /// <returns>MessagingExtensionActionResponse object.</returns>
        private MessagingExtensionActionResponse GetTaskModuleResponse(string title)
        {
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Url = _applicationBaseUrl + "/" + "ScheduleTask?title="+title+"&",
                        Height = 460,
                        Width = 600,
                        Title = "Schedule-task",
                    },
                },
            };
        }
    }
}