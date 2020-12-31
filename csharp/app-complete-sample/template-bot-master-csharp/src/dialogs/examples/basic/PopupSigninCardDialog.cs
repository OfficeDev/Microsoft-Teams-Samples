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
    /// This is PopUp SignIn Dialog Class. Main purpose of this class is to Display the PopUp SignIn Card
    /// </summary>

    [Serializable]
    public class PopupSigninCardDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogPopUpSignIn);

            var message = context.MakeMessage();
            var attachment = GetPopUpSignInCard();

            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            context.Done<object>(null);
        }

        private static Attachment GetPopUpSignInCard()
        {
            string baseUri = Convert.ToString(ConfigurationManager.AppSettings["BaseUri"]);

            var heroCard = new HeroCard
            {
                Title = Strings.PopUpSignInCardTitle,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Signin, Strings.PopUpSignInCardButtonTitle, value: baseUri + "/popUpSignin.html?height=200&width=200"),
                }
            };

            return heroCard.ToAttachment();
        }
    }
}