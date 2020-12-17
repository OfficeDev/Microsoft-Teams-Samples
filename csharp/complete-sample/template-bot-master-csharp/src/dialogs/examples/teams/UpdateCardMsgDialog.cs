using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is update card dialog class. Main purpose of this class is to update the card, if user has already setup the card message from below dialog file
    /// microsoft-teams-sample-complete-csharp\template-bot-master-csharp\src\dialogs\examples\teams\updatecardmsgsetupdialog.cs
    /// </summary>
    [Serializable]
    public class UpdateCardMsgDialog : IDialog<object>
    {
        public int updateCounter;

        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!string.IsNullOrEmpty(context.Activity.ReplyToId))
            {
                Activity activity = context.Activity as Activity;
                updateCounter = TemplateUtility.ParseUpdateCounterJson(activity);

                var updatedMessage = CreateUpdatedMessage(context);

                ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));

                try
                {
                    ResourceResponse resp = await client.Conversations.UpdateActivityAsync(context.Activity.Conversation.Id, context.Activity.ReplyToId, (Activity)updatedMessage);
                    await context.PostAsync(Strings.UpdateCardMessageConfirmation);
                }
                catch (Exception ex)
                {
                    await context.PostAsync(Strings.ErrorUpdatingCard + ex.Message);
                }
            }
            else
            {
                await context.PostAsync(Strings.NoMsgToUpdate);
            }

            context.Done<object>(null);

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogSetupUpdateCard);
        }

        #region Create Updated Card Message
        private IMessageActivity CreateUpdatedMessage(IDialogContext context)
        {
            var message = context.MakeMessage();
            var attachment = CreateUpdatedCardAttachment();
            message.Attachments.Add(attachment);
            return message;
        }

        private Attachment CreateUpdatedCardAttachment()    
        {
            return new HeroCard
            {
                Title = Strings.UpdatedCardTitle + " " + updateCounter,
                Subtitle = Strings.UpdatedCardSubTitle,
                Images = new List<CardImage> { new CardImage(ConfigurationManager.AppSettings["BaseUri"].ToString() + "/public/assets/computer_person.jpg") },
                Buttons = new List<CardAction>
                {
                   new CardAction(ActionTypes.MessageBack, Strings.UpdateCardButtonCaption, value: "{\"updateKey\": \"" + ++updateCounter + "\"}", text: DialogMatches.UpdateCard)
                }
            }.ToAttachment();
        }
        #endregion
    }
}