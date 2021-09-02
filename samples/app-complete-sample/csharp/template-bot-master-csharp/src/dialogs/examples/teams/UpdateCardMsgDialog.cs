using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is update card dialog class. Main purpose of this class is to update the card, if user has already setup the card message from below dialog file
    /// microsoft-teams-sample-complete-csharp\template-bot-master-csharp\src\dialogs\examples\teams\updatecardmsgsetupdialog.cs
    /// </summary>
    public class UpdateCardMsgDialog : ComponentDialog
    {
        public int updateCounter;
        public UpdateCardMsgDialog() : base(nameof(UpdateCardMsgDialog))
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
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }
            if (!string.IsNullOrEmpty(stepContext.Context.Activity.ReplyToId))
            {
                Activity activity = stepContext.Context.Activity as Activity;
                updateCounter = TemplateUtility.ParseUpdateCounterJson(activity);

                var updatedMessage = CreateUpdatedMessage(stepContext);

                ConnectorClient client = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

                try
                {
                    ResourceResponse resp = await client.Conversations.UpdateActivityAsync(stepContext.Context.Activity.Conversation.Id, stepContext.Context.Activity.ReplyToId, (Activity)updatedMessage);
                    await stepContext.Context.SendActivityAsync(Strings.UpdateCardMessageConfirmation);
                }
                catch (Exception ex)
                {
                    await stepContext.Context.SendActivityAsync(Strings.ErrorUpdatingCard + ex.Message);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(Strings.NoMsgToUpdate);
            }

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogSetupUpdateCard);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        #region Create Updated Card Message
        private IMessageActivity CreateUpdatedMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            var attachment = CreateUpdatedCardAttachment();
            message.Attachments = new List<Attachment>() { attachment };
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