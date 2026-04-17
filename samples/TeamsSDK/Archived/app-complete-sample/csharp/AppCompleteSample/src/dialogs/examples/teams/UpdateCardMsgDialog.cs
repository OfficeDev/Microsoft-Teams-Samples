using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using AppCompleteSample;
using AppCompleteSample.src.dialogs;
using AppCompleteSample.Utility;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is update card dialog class. Main purpose of this class is to update the card, if user has already setup the card message from below dialog file
    /// microsoft-teams-sample-complete-csharp\template-bot-master-csharp\src\dialogs\examples\teams\updatecardmsgsetupdialog.cs
    /// </summary>
    public class UpdateCardMsgDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        private readonly IOptions<AzureSettings> azureSettings;

        public int updateCounter;
        public UpdateCardMsgDialog(IStatePropertyAccessor<RootDialogState> conversationState, IOptions<AzureSettings> azureSettings) : base(nameof(UpdateCardMsgDialog))
        {
            this._conversationState = conversationState;
            this.azureSettings = azureSettings;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginUpdateCardMsgDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginUpdateCardMsgDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }
            var existingState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            if (!string.IsNullOrEmpty(stepContext.Context.Activity.ReplyToId))
            {
                IMessageActivity activity = stepContext.Context.Activity;

                updateCounter = TemplateUtility.ParseUpdateCounterJson((Activity)activity);

                var updatedMessage = CreateUpdatedMessage(stepContext);
                ConnectorClient client = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), this.azureSettings.Value.MicrosoftAppId, this.azureSettings.Value.MicrosoftAppPassword);

                try
                {
                    ResourceResponse resp = await client.Conversations.UpdateActivityAsync(stepContext.Context.Activity.Conversation.Id, stepContext.Context.Activity.ReplyToId, (Activity)updatedMessage);
                    await stepContext.Context.SendActivityAsync(Strings.UpdateCardMessageConfirmation);
                }
                catch (Exception ex)
                {
                    await stepContext.Context.SendActivityAsync(Strings.ErrorUpdatingCard + ex.Message);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(Strings.NoMsgToUpdate);
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogSetupUpdateCard;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        #region Create Updated Card Message
        private IMessageActivity CreateUpdatedMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            if (message.Attachments != null)
            {
                message.Attachments = null;
            }
            var attachment = CreateUpdatedCardAttachment();
            message.Attachments = new List<Attachment>() { attachment };
            return message;
        }
        private Attachment CreateUpdatedCardAttachment()
        {
            return new HeroCard
            {
                Title = Strings.UpdatedCardTitle + " " + updateCounter,
                Subtitle = Strings.UpdatedCardSubTitle,
                Images = new List<CardImage> { new CardImage(this.azureSettings.Value.BaseUri + " /public/assets/computer_person.jpg") },
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.MessageBack, Strings.UpdateCardButtonCaption, value: "{\"updateKey\": \"" + ++updateCounter + "\"}", text: DialogMatches.UpdateCard)
                }
            }.ToAttachment();
        }
        #endregion
    }
}