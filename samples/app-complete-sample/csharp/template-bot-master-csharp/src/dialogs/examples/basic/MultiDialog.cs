using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    public class MultiDialog1 : ComponentDialog
    {
        public MultiDialog1() : base(nameof(MultiDialog1))
        {
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
            await stepContext.Context.SendActivityAsync(Strings.HelpCaptionMultiDialog1);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    public class MultiDialog2 : ComponentDialog
    {
        public MultiDialog2() : base(nameof(MultiDialog2))
        {
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
            var message = CreateMultiDialog(stepContext);

            //Set the Last Dialog in Conversation Data
            //context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogMultiDialog2);

            await stepContext.Context.SendActivityAsync(message);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private IMessageActivity CreateMultiDialog(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            message.Attachments = new List<Attachment>
            {
                new HeroCard
            {
                Title = Strings.MultiDialogCardTitle,
                Subtitle = Strings.MultiDialogCardSubTitle,
                Text = Strings.MultiDialogCardText,
                Images = new List<CardImage> { new CardImage(ConfigurationManager.AppSettings["BaseUri"].ToString() + "/public/assets/computer_person.jpg") },
                Buttons = new List<CardAction>
                {
                   new CardAction("invoke", Strings.CaptionInvokeHelloDailog, value: "{\"" + Strings.InvokeRequestJsonKey + "\": \"" + Strings.cmdHelloDialog + "\"}"),
                   new CardAction("invoke", Strings.CaptionInvokeMultiDailog, value: "{\"" + Strings.InvokeRequestJsonKey+ "\": \"" + Strings.cmdMultiDialog1 + "\"}"),
                }
            }.ToAttachment()
        };
            return message;
        }
    }
}