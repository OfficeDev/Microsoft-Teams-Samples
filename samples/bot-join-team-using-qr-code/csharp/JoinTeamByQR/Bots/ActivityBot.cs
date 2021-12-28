// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using JoinTeamByQR.helper;
using JoinTeamByQR.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JoinTeamByQR.Bots
{
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        private readonly string _applicationBaseUrl;
        private readonly ConcurrentDictionary<string, string> _Token;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog, ConcurrentDictionary<string, string> token)
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            Dialog = dialog;
            _Token = token;
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
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userCommand = turnContext.Activity.Text.ToLower().Trim();

            if (userCommand == "generate" || userCommand == "logout"|| userCommand == "login")
            {
                // Run the Dialog with the new message Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Type 'generate' to get card with generate action."));                   
            }

            return;
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
            if (buttonType == "generate")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = _applicationBaseUrl + "/generate",
                        Height = 350,
                        Width = 350,
                        Title = "Join a Team",
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
            var teamInfo = JObject.FromObject(taskModuleRequest.Data);
            var teamId = (string)teamInfo.ToObject<ResponseData<string>>()?.TeamId;
            var userId = (string)teamInfo.ToObject<ResponseData<string>>()?.UserId;
            var token = string.Empty;
            _Token.TryGetValue("Token", out token);

            JoinTeamHelper.AddUserToTeam(token, teamId, userId);
            await turnContext.SendActivityAsync("User added to team successfully");
            return null;
        }
    }
}