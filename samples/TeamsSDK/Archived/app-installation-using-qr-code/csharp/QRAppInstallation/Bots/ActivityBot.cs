// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using QRAppInstallation.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace QRAppInstallation.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        private readonly string _connectionName;
        private readonly string _applicationBaseUrl;
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly IStatePropertyAccessor<TokenState> _TokenState;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            Dialog = dialog;
            _TokenState = conversationState.CreateProperty<TokenState>(nameof(TokenState));
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle task module is fetch
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
                        Url = _applicationBaseUrl + "/" + "GenerateQR",
                        Height = 350,
                        Width = 350,
                        Title = "Generate QR code",
                    },
                };
            }
            else if (buttonType == "install")
            {
                taskModuleResponse.Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = _applicationBaseUrl + "/" + "InstallApp",
                            Height = 350,
                            Width = 350,
                            Title = "Install App",
                        },
                };
            }

            return Task.FromResult(taskModuleResponse);
        }

        /// <summary>
        /// Handle task module is submit
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name = "taskModuleRequest" >The task module invoke request value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var appInfo = JObject.FromObject(taskModuleRequest.Data);
            var appId = (string)appInfo.ToObject<AppInstallDetails<string>>()?.AppId;
            var teamId = (string)appInfo.ToObject<AppInstallDetails<string>>()?.TeamId;
            var Token = await this._TokenState.GetAsync(turnContext, () => new TokenState());

            if (appInfo != null)
            {
                if (Token == null || string.IsNullOrEmpty(Token.AccessToken))
                {
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }

                var client = new SimpleGraphClient(Token.AccessToken);
                await client.InstallAppInTeam(teamId, appId);
                await turnContext.SendActivityAsync("App added sucessfully");
            }

           return null;
        }
    }
}