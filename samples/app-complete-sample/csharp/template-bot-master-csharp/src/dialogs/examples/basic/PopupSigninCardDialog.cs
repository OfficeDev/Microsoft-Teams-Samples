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
    /// <summary>
    /// This is PopUp SignIn Dialog Class. Main purpose of this class is to Display the PopUp SignIn Card
    /// </summary>
    public class PopupSigninCardDialog : ComponentDialog
    {
        public PopupSigninCardDialog() : base(nameof(PopupSigninCardDialog))
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

            //Set the Last Dialog in Conversation Data
            //stepContext.State.SetValue(Strings.LastDialogKey, Strings.LastDialogPopUpSignIn);

            string baseUri = Convert.ToString(ConfigurationManager.AppSettings["BaseUri"]);
            var message = stepContext.Context.Activity;
            message.Attachments = new List<Attachment> {
                new HeroCard
            {
                Title = Strings.PopUpSignInCardTitle,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Signin, Strings.PopUpSignInCardButtonTitle, value: baseUri + "/popUpSignin.html?height=200&width=200"),
                }
            }.ToAttachment()
            };
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}