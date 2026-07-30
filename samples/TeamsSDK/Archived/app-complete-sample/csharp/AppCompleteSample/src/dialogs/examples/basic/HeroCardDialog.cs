using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Hero Card Dialog Class. Main purpose of this class is to display the Hero Card example
    /// </summary>

    public class HeroCardDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public HeroCardDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(HeroCardDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginHeroCardDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginHeroCardDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogHeroCard;
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
            var attachment = GetHeroCard();
            message.Attachments = new List<Attachment>() { attachment };
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Attachment GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = Strings.HeroCardTitle,
                Subtitle = Strings.HeroCardSubTitle,
                Text = Strings.HeroCardTextMsg,
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, Strings.HeroCardButtonCaption, value: "https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-rich-card-attachments"),
                    new CardAction(ActionTypes.MessageBack, Strings.MessageBackCardButtonCaption, value: "{\"" + Strings.cmdValueMessageBack + "\": \"" + Strings.cmdValueMessageBack+ "\"}", text:Strings.cmdValueMessageBack, displayText:Strings.MessageBackDisplayedText)
                }
            };

            return heroCard.ToAttachment();
        }
    }
}