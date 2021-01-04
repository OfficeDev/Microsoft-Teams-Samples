using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Card Dialog Class. Main purpose of this dialog class is to post different types of Cards example like Hero Card, Thumbnail Card etc.
    /// </summary>
    [Serializable]
    public class DisplayCardsDialog : IDialog<object>
    {
        private IEnumerable<string> options = null;

        public DisplayCardsDialog()
        {
            options = new List<string> { Strings.DisplayCardHeroCard, Strings.DisplayCardThumbnailCard, Strings.DisplayCardO365ConnectorCardDefault, Strings.DisplayCardO365ConnectorCard2, Strings.DisplayCardO365ConnectorCard3, Strings.DisplayCardO365ConnectorActionableCardDefault, Strings.DisplayCardO365ConnectorActionableCard2, Strings.DisplayCardAdaptiveCard };
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogDisplayCardsDialog);

            await context.PostAsync(Strings.DisplayCardMsgTitle);

            PromptDialog.Choice<string>(
                context,
                this.DisplaySelectedCard,
                options,
                Strings.DisplayCardPromptText,
                Strings.PromptInvalidMsg,
                3);
        }

        public async Task DisplaySelectedCard(IDialogContext context, IAwaitable<string> result)
        {
            var selectedCard = await result;

            if (selectedCard.Equals(Strings.DisplayCardHeroCard))
            {
                context.Call(new HeroCardDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardThumbnailCard))
            {
                context.Call(new ThumbnailcardDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCardDefault))
            {
                context.Call(new O365ConnectorCardDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCard2))
            {
                context.Call(new O365ConnectorCardDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorCard3))
            {
                context.Call(new O365ConnectorCardDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorActionableCardDefault))
            {
                context.Call(new O365ConnectorCardActionsDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardO365ConnectorActionableCard2))
            {
                context.Call(new O365ConnectorCardActionsDialog(), ResumeAfterOptionDialog);
            }
            else if (selectedCard.Equals(Strings.DisplayCardAdaptiveCard))
            {
                context.Call(new AdaptiveCardDialog(), ResumeAfterOptionDialog);
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
                context.Done<object>(null);
            }
            catch (Exception ex)
            {
                await context.PostAsync(Strings.DisplayCardErrorMsg + ex.Message);
            }
        }
    }
}