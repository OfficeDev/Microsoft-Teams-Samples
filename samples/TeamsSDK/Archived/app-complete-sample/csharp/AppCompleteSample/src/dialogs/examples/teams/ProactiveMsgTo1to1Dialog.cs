using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using AppCompleteSample.src.dialogs;
using AppCompleteSample.Utility;
using AppCompleteSample;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Proactive Message Dialog Class. Main purpose of this class is to show the Send Proactive Message Example
    /// </summary>
    public class ProactiveMsgTo1to1Dialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        private readonly IOptions<AzureSettings> azureSettings;
        public ProactiveMsgTo1to1Dialog(IStatePropertyAccessor<RootDialogState> conversationState, IOptions<AzureSettings> azureSettings) : base(nameof(ProactiveMsgTo1to1Dialog))
        {
            this._conversationState = conversationState;
            this.azureSettings = azureSettings;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginProactiveMsgTo1to1DialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginProactiveMsgTo1to1DialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.Send1on1ConfirmMsg);
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogSend1on1Dialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            var userId = stepContext.Context.Activity.From.Id;
            var botId = stepContext.Context.Activity.Recipient.Id;
            var botName = stepContext.Context.Activity.Recipient.Name;

            var channelData = stepContext.Context.Activity.GetChannelData<TeamsChannelData>();
            var connectorClient = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), this.azureSettings.Value.MicrosoftAppId, this.azureSettings.Value.MicrosoftAppPassword);

            var parameters = new ConversationParameters
            {
                Bot = new ChannelAccount(botId, botName),
                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                ChannelData = new TeamsChannelData
                {
                    Tenant = channelData.Tenant
                }
            };

            var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);

            var message = Activity.CreateMessageActivity();
            message.From = new ChannelAccount(botId, botName);
            message.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
            message.Text = Strings.Send1on1Prompt;

            await connectorClient.Conversations.SendToConversationAsync((Activity)message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}