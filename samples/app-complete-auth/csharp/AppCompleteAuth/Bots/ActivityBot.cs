// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AppCompleteAuth.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Bots
{
    public class ActivityBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        private readonly string _applicationBaseUrl;

        public ActivityBot(IConfiguration configuration, ConversationState conversationState, T dialog)
        {
            _applicationBaseUrl = configuration["ApplicationBaseUrl"] ?? throw new NullReferenceException("ApplicationBaseUrl");
            ConversationState = conversationState;
            Dialog = dialog;
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
            if (turnContext.Activity.Text != null)
            {
                var userCommand = turnContext.Activity.Text.ToLower().Trim();

                if (userCommand == "sso" || userCommand == "logout" || userCommand == "otheridentityprovider" || userCommand == "usingcredentials")
                {
                    // Run the Dialog with the new message Activity.
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(GetLoginOptionCard()));
                }
            }

            return;
        }

        /// <summary>
        /// Invoked when the user askfor sign in.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(turnContext.Activity.Value);
            var state = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.State;

            if (state.ToString() == "CancelledByUser")
            {
                await turnContext.SendActivityAsync("Sign in cancelled by user");
            }
            else
            {
                var cred = JObject.Parse(state);
                var userName = (string)cred.ToObject<CardTaskFetchValue<string>>()?.UserName;
                var password = (string)cred.ToObject<CardTaskFetchValue<string>>()?.Password;
                if (userName == Constant.UserName && password == Constant.Password)
                {
                    await turnContext.SendActivityAsync("Authentication Successful");
                }
                else
                {
                    await turnContext.SendActivityAsync("Invalid username or password");
                }    
            } 
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
            await turnContext.SendActivityAsync("File uploaded successfully");
            return null;
        }

        private static Attachment GetLoginOptionCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Login options",
                Text = "Select a login option",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack,title:"Using credentials", value: "usingcredentials", text:"usingcredentials", displayText:"Using credentials"),
                    new CardAction(ActionTypes.MessageBack,title:"Other identity provider", value: "otheridentityprovider", text:"otheridentityprovider", displayText:"Other identity provider"),
                    new CardAction(ActionTypes.MessageBack,title:"SSO authentication", value: "sso", text:"sso", displayText:"SSO authentication")
                }
            };

            return heroCard.ToAttachment();
        }
    }
}