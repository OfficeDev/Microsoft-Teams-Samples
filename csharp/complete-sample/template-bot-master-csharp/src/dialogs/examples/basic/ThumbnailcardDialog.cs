using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Thumbnail Card Dialog Class. Main purpose of this class is to display the Thumbnail Card example
    /// </summary>

    [Serializable]
    public class ThumbnailcardDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogThumbnailCard);

            var message = context.MakeMessage();
            var attachment = GetThumbnailCard();

            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            context.Done<object>(null);
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