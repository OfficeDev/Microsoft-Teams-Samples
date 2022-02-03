using AppCompleteAuth.helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Dialogs
{
    public class FacebookAuthDialog : ComponentDialog
    {
        private readonly string ConnectionName;

        public FacebookAuthDialog(string configuration) : base(nameof(FacebookAuthDialog))
        {
            ConnectionName = configuration;
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Login to facebook",
                    Title = "Log In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        // Shows the OAuthPrompt to the user to login if not already logged in.
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Getting the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                // Getting basic facebook profile details.
                FacebookProfile profile = await FacebookHelper.GetFacebookProfileName(tokenResponse.Token);
                var message = CreateFBMessage(stepContext, profile);

                await stepContext.Context.SendActivityAsync(message);

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private Attachment CreateFBProfileCard(FacebookProfile profile)
        {
            return new ThumbnailCard
            {
                Title = "" + " " + profile.Name,
                Images = new List<CardImage> { new CardImage(profile.ProfilePicture.data.url) },
            }.ToAttachment();
        }

        private IMessageActivity CreateFBMessage(WaterfallStepContext context, FacebookProfile profile)
        {
            var message = context.Context.Activity;
            var attachment = CreateFBProfileCard(profile);
            message.Attachments = new List<Attachment> { attachment };
            return message;
        }
    }
}
