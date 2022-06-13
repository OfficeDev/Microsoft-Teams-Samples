// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCatalogSample;
using AppCatalogSample.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace AppCatalogSample.Bots
{
    // This bot is derived (view DialogBot<T>) from the TeamsACtivityHandler class currently included as part of this sample.

    public class AppCatalogBot<T> : DialogBot<T> where T : Dialog
    {

        public AppCatalogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            string WelcomeText = "AppCatalog";
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to  Bot {member.Name} {WelcomeText}",
                        cancellationToken: cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
            }
        }

        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

            // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.

            // Run the Dialog with the new Invoke Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
        protected static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Your Action :" + " \r" + "-" + " Publish: publish" + " \r" + "-" + " Update: update" + " \r" + "-" + " Delete: delete" + " \r" + " \r" + "-" + " ListApp: listapp" + " \r" + "-" + " ListApp by ID: app" + " \r" + "-" + " App based on manifest Id: findapp" + " \r" + "-" + " App Status:status" + " \r" + "-" + " List of bot:bot" + " \r");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Update", Type = ActionTypes.ImBack, Value = "update"},
                    new CardAction() { Title = "Publish", Type = ActionTypes.ImBack, Value = "publish"},
                    new CardAction() { Title = "Delete", Type = ActionTypes.ImBack, Value = "delete"},
                    new CardAction() { Title = "ListApp", Type = ActionTypes.ImBack, Value = "listapp"},
                    new CardAction() { Title = "ListApp by ID", Type = ActionTypes.ImBack, Value = "app" },
                    new CardAction() { Title = "App based on manifest Id", Type = ActionTypes.ImBack, Value = "findapp"},
                    new CardAction() { Title = "App Status", Type = ActionTypes.ImBack, Value = "status"},
                    new CardAction() { Title = "List of bot", Type = ActionTypes.ImBack, Value = "bot" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }

}