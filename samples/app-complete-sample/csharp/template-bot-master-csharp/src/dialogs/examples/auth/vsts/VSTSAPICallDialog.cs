using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp
{
    /// <summary>
    /// This Dialog implements the OAuth login flow for VSTS. 
    /// You can read more about VSTS login flow here: https://www.visualstudio.com/en-us/docs/integrate/extensions/overview
    /// </summary>
    [Serializable]
    public class VSTSAPICallDialog : ComponentDialog
    {

        /// <summary>
        /// OAuth callback registered for Facebook app.
        /// <see cref="Controllers.OAuthCallbackVSTSController"/> implementats the callback.
        /// </summary>
        /// <remarks>
        /// Make sure to replace this with the appropriate website url registered for your VSTS app.
        /// </remarks>
        public static readonly Uri VSTSOauthCallback = new Uri(ConfigurationManager.AppSettings["CallbackUrl"].ToString());

        /// <summary>
        /// The key that is used to keep the AccessToken in <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.UserData"/>
        /// </summary>
        public static readonly string VSTSAuthTokenKey = ConfigurationManager.AppSettings["VSTSAuthToken"].ToString();

        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        protected readonly IStatePropertyAccessor<PrivateConversationData> _privateCoversationState;
        public VSTSAPICallDialog(IStatePropertyAccessor<RootDialogState> conversationState, IStatePropertyAccessor<PrivateConversationData> privateCoversationState) : base(nameof(SimpleFacebookAuthDialog))
        {
            this._conversationState = conversationState;
            this._privateCoversationState = privateCoversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginFormflowAsync,
                SaveResultAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> BeginFormflowAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogVSTSDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);
            await LogIn(stepContext);

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions
                    {
                        Prompt = new Activity(),
                    },
                    cancellationToken);
        }

        private async Task<DialogTurnResult> SaveResultAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = stepContext.Result as IMessageActivity;
            VSTSAcessToken facebookToken = new VSTSAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(stepContext.Context, () => new PrivateConversationData());
            if (currentPrivateConversationData.PersistedCookieVSTS!=null)
            {
                magicNumber = currentPrivateConversationData.PersistedCookieVSTS.User.Properties["MagicNumber"].ToString();

                if (string.Equals(msg.Text, magicNumber))
                {
                    currentPrivateConversationData.PersistedCookieVSTS.User.Properties["IsValidated"] = true;

                    await this._privateCoversationState.SetAsync(stepContext.Context, currentPrivateConversationData);

                    token = currentPrivateConversationData.PersistedCookieVSTS.User.Properties["AccessToken"].ToString();

                    // Dialog is resumed by the OAuth callback and access token
                    // is encoded in the message.Text
                    var valid = Convert.ToBoolean(currentPrivateConversationData.PersistedCookieVSTS.User.Properties["IsValidated"]);

                    if (valid)
                    {
                        var name = currentPrivateConversationData.PersistedCookieVSTS.User.Properties["UserName"].ToString();
                        currentPrivateConversationData.Name = name;
                        currentPrivateConversationData.VSTSAuthTokenKey = token;
                        await this._privateCoversationState.SetAsync(stepContext.Context, currentPrivateConversationData);
                        await stepContext.Context.SendActivityAsync(Strings.VSTSLoginSuccessPrompt);
                        await stepContext.Context.SendActivityAsync(Strings.VSTSlogoutPrompt);
                        return await stepContext.EndDialogAsync();
                    }
                    return await stepContext.ContinueDialogAsync();

                }
                else
                {
                    //When entered number is not valid
                    await stepContext.Context.SendActivityAsync(Strings.AuthMagicNumberNotMacthed);
                    await LogIn(stepContext);
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                await LogIn(stepContext);
                return await stepContext.ContinueDialogAsync();
            }
        }

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(WaterfallStepContext context)
        {
            string token;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(context.Context, () => new PrivateConversationData());
            token = currentPrivateConversationData.VSTSAuthTokenKey;
            if (currentPrivateConversationData.VSTSAuthTokenKey!=null)
            {
                var conversationReference = context.Context.Activity.GetConversationReference();
                currentPrivateConversationData.PersistedCookieVSTS = conversationReference;
                await this._privateCoversationState.SetAsync(context.Context, currentPrivateConversationData);

                // sending the sigin card with Facebook login url
                var reply = context.Context.Activity;
                var vstsLoginUrl = VSTSHelpers.GenerateAuthorizeUrl(conversationReference);

                reply.Text = Strings.VSTSLoginTitle;

                //Login Card
                var loginCard = new HeroCard
                {
                    Title = Strings.VSTSLoginCardTitle,
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, Strings.VSTSLoginCardButtonCaption, value: vstsLoginUrl) }
                };

                reply.Attachments.Add(loginCard.ToAttachment());

                await context.Context.SendActivityAsync(reply);
            }
            else
            {
                await context.Context.SendActivityAsync(Strings.VSTSLoginSessionExistsPrompt);
                await context.Context.SendActivityAsync(Strings.VSTSlogoutPrompt);
                await context.EndDialogAsync(token);
            }
        }
    }
}