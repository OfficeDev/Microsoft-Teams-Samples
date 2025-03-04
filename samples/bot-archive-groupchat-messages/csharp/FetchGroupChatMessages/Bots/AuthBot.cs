// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace FetchGroupChatMessagesWithRSC.Bots
{
    /// <summary>
    /// Auth bot for handling user authentication and interactions.
    /// </summary>
    public class AuthBot<T> : ActivityBot<T> where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly ConversationState _conversationState;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthBot{T}"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="env">The web host environment.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="conversationState">The conversation state.</param>
        /// <param name="dialog">The dialog instance.</param>
        public AuthBot(IConfiguration configuration, IWebHostEnvironment env, IHttpClientFactory clientFactory, ConversationState conversationState, T dialog)
            : base(configuration, env, clientFactory, conversationState, dialog)
        {
            _dialog = dialog;
            _conversationState = conversationState;
        }

        /// <summary>
        /// Invoked when the bot is added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! Please type 'login' to initiate the authentication flow."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Invoked when a token response event is received.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new Token Response Event Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /// <summary>
        /// Invoked when a sign-in verify state activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new Teams Sign-in Verify State Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
