using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Roster Payload Dialog Class. Main purpose of this dialog class is to Call the Roster Api and Post the 
    /// full JSON Payload in Teams returned by Roster Api.
    /// </summary>
    public class FetchRosterDialog : ComponentDialog
    {
        public FetchRosterDialog() : base(nameof(FetchRosterDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
                SaveResultAsync,
            }));
        }

            private async Task<DialogTurnResult> BeginFormflowAsync(
    WaterfallStepContext stepContext,
    CancellationToken cancellationToken = default(CancellationToken))
            {
                await stepContext.Context.SendActivityAsync(Strings.RosterWelcomeMsgTitle);

                // Begin the Formflow dialog.
                return await stepContext.BeginDialogAsync(
                    nameof(FetchRosterDialog),
                    cancellationToken: cancellationToken);
            }

        private async Task<DialogTurnResult> SaveResultAsync(
    WaterfallStepContext stepContext,
    CancellationToken cancellationToken = default(CancellationToken))
        {
            var connectorClient = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl));

            var response = await connectorClient.Conversations.GetConversationMembersAsync(stepContext.Context.Activity.Conversation.Id);
            string output = JsonConvert.SerializeObject(response);

            var message = stepContext.MakeMessage();
            message.Text = output;

            //Set the Last Dialog in Conversation Data
            //stepContext.context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchPayloadRosterDialog);

            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}