
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AppCompleteSample;
using AppCompleteSample.src.dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is setup card dialog class. Main purpose of this class is to setup the card message and then user can update the card using below update dialog file
    /// microsoft-teams-sample-complete-csharp\template-bot-master-csharp\src\dialogs\examples\teams\UpdateCardMsgDialog.cs
    /// </summary>
    public class UpdateCardMsgSetupDialog : ComponentDialog
    {
        public int updateCounter = 0;
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public UpdateCardMsgSetupDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(UpdateCardMsgSetupDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginUpdateCardMsgSetupDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginUpdateCardMsgSetupDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            var message = SetupMessage(stepContext);
            await stepContext.Context.SendActivityAsync(message);

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogSetupUpdateCard;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        #region Create Message to Setup Card
        private IMessageActivity SetupMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            if (message.Attachments != null)
            {
                message.Attachments = null;
            }

            if (message.Entities.Count >= 1)
            {
                message.Entities.Remove(message.Entities[0]);
            }
            var attachment = CreateCard();
            message.Attachments = new List<Attachment>() { attachment };
            return message;
        }

        private Attachment CreateCard()
        {
            return new HeroCard
            {
                Title = Strings.SetUpCardTitle,
                Subtitle = Strings.SetupCardSubTitle,
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack, Strings.UpdateCardButtonCaption, value: "{\"updateKey\": \"" + ++updateCounter + "\"}", text: DialogMatches.UpdateCard)
                }
            }.ToAttachment();
        }
        #endregion
    }
}