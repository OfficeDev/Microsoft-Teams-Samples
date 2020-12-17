using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    [Serializable]
    public class MultiDialog1 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogMultiDialog1);

            await context.PostAsync(Strings.HelpCaptionMultiDialog1);
            context.Done<object>(null);
        }
    }

    [Serializable]
    public class MultiDialog2 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var message = CreateMultiDialog(context);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogMultiDialog2);

            await context.PostAsync(message);
            context.Done<object>(null);
        }

        private IMessageActivity CreateMultiDialog(IDialogContext context)
        {
            var message = context.MakeMessage();
            var attachment = CreateMultiDialogCard();
            message.Attachments.Add(attachment);
            return message;
        }

        private Attachment CreateMultiDialogCard()
        {
            return new HeroCard
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
            }.ToAttachment();
        }
    }
}