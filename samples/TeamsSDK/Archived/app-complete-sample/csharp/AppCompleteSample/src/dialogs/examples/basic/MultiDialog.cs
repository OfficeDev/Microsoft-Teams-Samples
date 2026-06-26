using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using AppCompleteSample.src;
using AppCompleteSample.Utility;
using AppCompleteSample;
using System.Threading.Tasks;
using System.Threading;
using AppCompleteSample.src.dialogs;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    public class MultiDialog1 : ComponentDialog
    {
        public MultiDialog1() : base(nameof(MultiDialog1))
        {
            InitialDialogId = nameof(WaterfallDialog);
           
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginMultiDialog1Async,
            }));
        }

        private async Task<DialogTurnResult> BeginMultiDialog1Async(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await stepContext.Context.SendActivityAsync(Strings.HelpCaptionMultiDialog1);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    public class MultiDialog2 : ComponentDialog
    {
        private readonly IOptions<AzureSettings> azureSettings;

        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public MultiDialog2(IStatePropertyAccessor<RootDialogState> conversationState, IOptions<AzureSettings> azureSettings) : base(nameof(MultiDialog2))
        {
            this._conversationState = conversationState;
            this.azureSettings = azureSettings;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginMultiDialog2Async,
            }));
        }

        private async Task<DialogTurnResult> BeginMultiDialog2Async(
WaterfallStepContext stepContext,
CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = CreateMultiDialog(stepContext);

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogMultiDialog2;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            await stepContext.Context.SendActivityAsync(message);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private IMessageActivity CreateMultiDialog(WaterfallStepContext context)
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
            var attachment = CreateMultiDialogCard();
            message.Attachments = new List<Attachment>() { attachment };
            return message;
        }

        private Attachment CreateMultiDialogCard()
        {
            return new HeroCard
            {
                Title = Strings.MultiDialogCardTitle,
                Subtitle = Strings.MultiDialogCardSubTitle,
                Text = Strings.MultiDialogCardText,
                Images = new List<CardImage> { new CardImage(this.azureSettings.Value.BaseUri + " /public/assets/computer_person.jpg") },
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.ImBack, Strings.CaptionInvokeHelloDailog, value: Strings.cmdHelloDialog),
                   new CardAction(ActionTypes.ImBack, Strings.CaptionInvokeMultiDailog, value: Strings.cmdMultiDialog1),
                }
            }.ToAttachment();
        }
    }
}