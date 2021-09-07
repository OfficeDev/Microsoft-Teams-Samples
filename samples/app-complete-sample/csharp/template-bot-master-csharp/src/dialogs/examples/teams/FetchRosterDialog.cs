using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using Newtonsoft.Json;
using System;
using System.Configuration;
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
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        public FetchRosterDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(FetchRosterDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFetchRosterDialogAsync
            }));
        }

            private async Task<DialogTurnResult> BeginFetchRosterDialogAsync(
                WaterfallStepContext stepContext,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                await stepContext.Context.SendActivityAsync(Strings.RosterWelcomeMsgTitle);

            var connectorClient = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

            var response = await connectorClient.Conversations.GetConversationMembersAsync(stepContext.Context.Activity.Conversation.Id);
            string output = JsonConvert.SerializeObject(response);

            var message = stepContext.MakeMessage();
            message.Text = output;

            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogFetchPayloadRosterDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}