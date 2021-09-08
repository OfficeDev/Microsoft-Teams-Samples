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
    /// This Dialog implements the OAuth login flow for Facebook. 
    /// You can read more about Facebook's login flow here: https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow
    /// </summary>
    public class SimpleFacebookAuthDialog : ComponentDialog
    {
        /// <summary>
        /// OAuth callback registered for Facebook app.
        /// <see cref="Controllers.OAuthCallbackController"/> implementats the callback.
        /// </summary>
        /// <remarks>
        /// Make sure to replace this with the appropriate website url registered for your Facebook app.
        /// </remarks>
        public static readonly Uri FacebookOauthCallback = new Uri(ConfigurationManager.AppSettings["FBCallbackUrl"].ToString());

        /// <summary>
        /// The key that is used to keep the AccessToken in <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PrivateConversationData"/>
        /// </summary>
        public static readonly string AuthTokenKey = ConfigurationManager.AppSettings["FBAuthToken"].ToString();

        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        protected readonly IStatePropertyAccessor<PrivateConversationData> _privateCoversationState;
        public SimpleFacebookAuthDialog(IStatePropertyAccessor<RootDialogState> conversationState, IStatePropertyAccessor<PrivateConversationData> privateCoversationState) : base(nameof(SimpleFacebookAuthDialog))
        {
            this._conversationState = conversationState;
            this._privateCoversationState = privateCoversationState;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = "fbconnection",
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
                DisplayTokenPhase1Async,
                DisplayTokenPhase2Async,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            //AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            //{
            //    BeginFormflowAsync,
            //    SaveResultAsync,
            //}));
            //AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                // Pull in the data from the Microsoft Graph.
                //var client = new SimpleGraphClient(tokenResponse.Token);
                //var me = await client.GetMeAsync();
                //var title = !string.IsNullOrEmpty(me.JobTitle) ?
                //            me.JobTitle : "Unknown";

                //await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); you job title is: {title}");
                await stepContext.Context.SendActivityAsync($"You're logged in as ");

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                // Call the prompt again because we need the token. The reasons for this are:
                // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
                // about refreshing it. We can always just call the prompt again to get the token.
                // 2. We never know how long it will take a user to respond. By the time the
                // user responds the token may have expired. The user would then be prompted to login again.
                //
                // There is no reason to store the token locally in the bot because we can always just call
                // the OAuth prompt to get the token or get a new token if needed.
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> BeginFormflowAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogFacebookDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);
            await LogIn(stepContext);

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Strings.OAuthCallbackUserPrompt),
                    },
                    cancellationToken);
        }

        private async Task<DialogTurnResult> SaveResultAsync(
             WaterfallStepContext stepContext,
             CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = stepContext.Result as string;
            FacebookAcessToken facebookToken = new FacebookAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(stepContext.Context, () => new PrivateConversationData());

            if (currentPrivateConversationData.PersistedCookie !=null)
            {
                magicNumber = currentPrivateConversationData.PersistedCookie.User.Properties[ConfigurationManager.AppSettings["FBMagicNumberKey"].ToString()].ToString();

                if (string.Equals(msg, magicNumber))
                {
                    currentPrivateConversationData.PersistedCookie.User.Properties[ConfigurationManager.AppSettings["FBIsValidatedKey"].ToString()] = true;
                    await this._privateCoversationState.SetAsync(stepContext.Context, currentPrivateConversationData);

                    token = currentPrivateConversationData.PersistedCookie.User.Properties[ConfigurationManager.AppSettings["FBAccessTokenKey"].ToString()].ToString();
                    currentPrivateConversationData.AuthTokenKey = token;
                    var valid = await FacebookHelpers.ValidateAccessToken(token);

                    if (valid)
                    {
                        FacebookProfile profile = await FacebookHelpers.GetFacebookProfileName(token);
                        var message = CreateFBMessage(stepContext, profile);

                        await stepContext.Context.SendActivityAsync(message);
                        await stepContext.Context.SendActivityAsync(Strings.FBLginSuccessPromptLogoutInfo);

                        await this._privateCoversationState.SetAsync(stepContext.Context, currentPrivateConversationData);
                        return await stepContext.EndDialogAsync(token);
                    }
                    return await stepContext.EndDialogAsync(token);
                }
                else
                {
                    //When entered number is not valid
                    await stepContext.Context.SendActivityAsync(Strings.AuthMagicNumberNotMacthed);
                    await LogIn(stepContext);
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                await LogIn(stepContext);
                return await stepContext.EndDialogAsync();
            }
        }

        #region Create FB Profile Message Card
        private Attachment CreateFBProfileCard(FacebookProfile profile)
        {
            return new ThumbnailCard
            {
                Title = Strings.FBLoginSuccessPrompt + " " + profile.Name + "(" + profile.Gender + ")",
                Images = new List<CardImage> { new CardImage(profile.ProfilePicture.data.url) },
                Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl, Strings.FBCardButtonCaption, value: profile.link)
                    }
            }.ToAttachment();
        }

        private IMessageActivity CreateFBMessage(WaterfallStepContext context, FacebookProfile profile)
        {
            var message = context.Context.Activity;
            var attachment = CreateFBProfileCard(profile);
            message.Attachments = new List<Attachment> { attachment };
            return message;
        }
        #endregion

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(WaterfallStepContext context)
        {
            string token;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(context.Context, () => new PrivateConversationData());
            token = currentPrivateConversationData.AuthTokenKey;
            if (currentPrivateConversationData.AuthTokenKey == null)
            {
                var conversationReference = context.Context.Activity.GetConversationReference();
                currentPrivateConversationData.PersistedCookie = conversationReference;
                await this._privateCoversationState.SetAsync(context.Context, currentPrivateConversationData);

                // sending the sigin card with Facebook login url
                var reply = context.Context.Activity;
                var fbLoginUrl = FacebookHelpers.GetFacebookLoginURL(conversationReference, FacebookOauthCallback.ToString());
                reply.Text = Strings.FBLoginTitle;

                //Login Card
                var loginCard = new HeroCard
                {
                    Title = Strings.FBLoginCardPrompt,
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, Strings.FBLoginCardButtonCaption, value: fbLoginUrl) }
                }.ToAttachment();

                reply.Attachments = new List<Attachment> { loginCard };

                await context.Context.SendActivityAsync(reply);
            }
            else
            {
                await context.Context.SendActivityAsync(Strings.FBLoginSessionExistsPrompt);
                await context.Context.SendActivityAsync(Strings.FBLogoutPrompt);
                await context.EndDialogAsync(token);
            }
        }
    }
}