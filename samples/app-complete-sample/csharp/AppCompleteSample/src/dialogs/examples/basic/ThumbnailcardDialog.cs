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
    /// This is Thumbnail Card Dialog Class. Main purpose of this class is to display the Thumbnail Card example
    /// </summary>
    public class ThumbnailcardDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public ThumbnailcardDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(ThumbnailcardDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginThumbnailcardDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginThumbnailcardDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogThumbnailCard;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            var message = stepContext.Context.Activity;
            if (message.Attachments != null)
            {
                message.Attachments = null;
            }

            if (message.Entities.Count >= 1)
            {
                message.Entities.Remove(message.Entities[0]);
            }
            var attachment = GetThumbnailCard();
            message.Attachments = new List<Attachment>() { attachment };
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Attachment GetThumbnailCard()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = Strings.ThumbnailCardTitle,
                Subtitle = Strings.ThumbnailCardSubTitle,
                Text = Strings.ThumbnailCardTextMsg,
                Images = new List<CardImage> { new CardImage(Strings.ThumbnailCardImageUrl) },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, Strings.ThumbnailCardButtonCaption, value: "https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-rich-card-attachments"),
                    new CardAction(ActionTypes.MessageBack, Strings.MessageBackCardButtonCaption, value: "{\"" + Strings.cmdValueMessageBack + "\": \"" + Strings.cmdValueMessageBack+ "\"}", text:Strings.cmdValueMessageBack, displayText:Strings.MessageBackDisplayedText)
                }
            };

            return thumbnailCard.ToAttachment();
        }
    }
}