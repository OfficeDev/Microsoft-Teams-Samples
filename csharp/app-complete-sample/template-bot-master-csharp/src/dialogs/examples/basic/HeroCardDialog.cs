using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Hero Card Dialog Class. Main purpose of this class is to display the Hero Card example
    /// </summary>

    [Serializable]
    public class HeroCardDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogHeroCard);

            var message = context.MakeMessage();
            var attachment = GetHeroCard();

            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            context.Done<object>(null);
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