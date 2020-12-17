using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp
{
    /// <summary>
    /// This Dialog implements the OAuth login flow for Facebook. 
    /// You can read more about Facebook's login flow here: https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow
    /// </summary>
    [Serializable]
    public class SimpleFacebookAuthDialog : IDialog<string>
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

        public async Task StartAsync(IDialogContext context)
        {
            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFacebookDialog);

            await LogIn(context);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            ConversationReference conversationReference;
            FacebookAcessToken facebookToken = new FacebookAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;

            if (context.PrivateConversationData.TryGetValue("persistedCookie", out conversationReference))
            {
                magicNumber = conversationReference.User.Properties[ConfigurationManager.AppSettings["FBMagicNumberKey"].ToString()].ToString();

                if (string.Equals(msg.Text, magicNumber))
                {
                    conversationReference.User.Properties[ConfigurationManager.AppSettings["FBIsValidatedKey"].ToString()] = true;
                    context.PrivateConversationData.SetValue("persistedCookie", conversationReference);

                    token = conversationReference.User.Properties[ConfigurationManager.AppSettings["FBAccessTokenKey"].ToString()].ToString();

                    var valid = await FacebookHelpers.ValidateAccessToken(token);

                    if(valid)
                    {
                        FacebookProfile profile = await FacebookHelpers.GetFacebookProfileName(token);
                        var message = CreateFBMessage(context, profile);

                        await context.PostAsync(message);
                        await context.PostAsync(Strings.FBLginSuccessPromptLogoutInfo);

                        context.PrivateConversationData.SetValue(AuthTokenKey, token);
                        context.Done(token);
                    }
                }
                else
                {
                    //When entered number is not valid
                    await context.PostAsync(Strings.AuthMagicNumberNotMacthed);
                    await LogIn(context);
                }
            }
            else
            {
                await LogIn(context);
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

        private IMessageActivity CreateFBMessage(IDialogContext context, FacebookProfile profile)
        {
            var message = context.MakeMessage();
            var attachment = CreateFBProfileCard(profile);
            message.Attachments.Add(attachment);
            return message;
        }
        #endregion

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                var conversationReference = context.Activity.ToConversationReference();

                context.PrivateConversationData.SetValue("persistedCookie", conversationReference);

                // sending the sigin card with Facebook login url
                var reply = context.MakeMessage();
                var fbLoginUrl = FacebookHelpers.GetFacebookLoginURL(conversationReference, FacebookOauthCallback.ToString());
                reply.Text = Strings.FBLoginTitle;

                //Login Card
                var loginCard = new HeroCard
                {
                    Title = Strings.FBLoginCardPrompt,
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, Strings.FBLoginCardButtonCaption, value: fbLoginUrl) }
                };

                reply.Attachments.Add(loginCard.ToAttachment());

                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync(Strings.FBLoginSessionExistsPrompt);
                await context.PostAsync(Strings.FBLogoutPrompt);
                context.Done(token);
            }
        }
    }
}