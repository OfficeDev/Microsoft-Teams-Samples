using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Update Text Dialog Class. Main purpose of this class is to Update the Text in Bot
    /// </summary>
    public class UpdateTextMsgDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public UpdateTextMsgDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(UpdateTextMsgDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginFormflowAsync(
WaterfallStepContext stepContext,
CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            string cachedMessage = string.Empty;

            if (stepContext.State.TryGetValue(Strings.SetUpMsgKey, out cachedMessage))
            {
                IMessageActivity reply = stepContext.Context.Activity;
                reply.Text = Strings.UpdateMessagePrompt;

                ConnectorClient client = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
                ResourceResponse resp = await client.Conversations.UpdateActivityAsync(stepContext.Context.Activity.Conversation.Id, cachedMessage, (Activity)reply);

                await stepContext.Context.SendActivityAsync(Strings.UpdateMessageConfirmation);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(Strings.ErrorTextMessageUpdate);
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogUpdateMessasge;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}