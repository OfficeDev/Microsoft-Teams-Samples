using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is setup card dialog class. Main purpose of this class is to setup the card message and then user can update the card using below update dialog file
    /// microsoft-teams-sample-complete-csharp\template-bot-master-csharp\src\dialogs\examples\teams\UpdateCardMsgDialog.cs
    /// </summary>
    [Serializable]
    public class UpdateCardMsgSetupDialog : IDialog<object>
    {
        public int updateCounter = 0;
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var message = SetupMessage(context);
            await context.PostAsync(message);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogSetupUpdateCard);

            context.Done<object>(null);
        }

        #region Create Message to Setup Card
        private IMessageActivity SetupMessage(IDialogContext context)
        {
            var message = context.MakeMessage();
            var attachment = CreateCard();
            message.Attachments.Add(attachment);
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