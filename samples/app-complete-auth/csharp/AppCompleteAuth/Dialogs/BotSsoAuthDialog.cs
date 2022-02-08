using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Dialogs
{
    public class BotSsoAuthDialog : LogoutDialog
    {
        public BotSsoAuthDialog(string configuration) : base(nameof(BotSsoAuthDialog), configuration)
        {
            AddDialog(new TokenExchangeOAuthPrompt(
                 nameof(TokenExchangeOAuthPrompt),
                 new OAuthPromptSettings
                 {
                     ConnectionName = ConnectionName,
                     Text = "Please login",
                     Title = "Sign In",
                     Timeout = 1000 * 60 * 1, // User has 5 minutes to login (1000 * 60 * 5)

                    //EndOnInvalidMessage = true
                }));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
        }

        // Invoked after success of prompt step async.
        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step.
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);

                // Pull in the data from the Microsoft Graph.
                var client = new SimpleGraphClient(tokenResponse.Token);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); you job title is: {title}; your photo is: ");
                var photo = await client.GetPhotoAsync();
                var cardImage = new CardImage(photo);
                var card = new ThumbnailCard(images: new List<CardImage>() { cardImage });
                var reply = MessageFactory.Attachment(card.ToAttachment());
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.EndDialogAsync();
            }

            return await stepContext.EndDialogAsync();
        }
    }
}
