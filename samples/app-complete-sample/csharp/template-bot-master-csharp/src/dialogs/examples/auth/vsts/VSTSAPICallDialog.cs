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
    /// This Dialog implements the OAuth login flow for VSTS. 
    /// You can read more about VSTS login flow here: https://www.visualstudio.com/en-us/docs/integrate/extensions/overview
    /// </summary>
    [Serializable]
    public class VSTSAPICallDialog : IDialog<string>
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

        public async Task StartAsync(IDialogContext context)
        {
            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogVSTSDialog);

            await LogIn(context);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            ConversationReference conversationReference;
            VSTSAcessToken facebookToken = new VSTSAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;

            if (context.UserData.TryGetValue("persistedCookieVSTS", out conversationReference))
            {
                magicNumber = conversationReference.User.Properties["MagicNumber"].ToString();

                if (string.Equals(msg.Text, magicNumber))
                {
                    conversationReference.User.Properties["IsValidated"] = true;

                    context.UserData.SetValue("persistedCookieVSTS", conversationReference);
                    
                    token = conversationReference.User.Properties["AccessToken"].ToString();

                    // Dialog is resumed by the OAuth callback and access token
                    // is encoded in the message.Text
                    var valid = Convert.ToBoolean(conversationReference.User.Properties["IsValidated"]);

                    if (valid)
                    {
                        var name = conversationReference.User.Properties["UserName"].ToString();
                        context.UserData.SetValue("name", name);
                        await context.PostAsync(Strings.VSTSLoginSuccessPrompt);
                        await context.PostAsync(Strings.VSTSlogoutPrompt);
                        context.UserData.SetValue(VSTSAuthTokenKey, token);
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

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.UserData.TryGetValue(VSTSAuthTokenKey, out token))
            {
                var conversationReference = context.Activity.ToConversationReference();

                context.UserData.SetValue("persistedCookieVSTS", conversationReference);

                // sending the sigin card with Facebook login url
                var reply = context.MakeMessage();
                var vstsLoginUrl = VSTSHelpers.GenerateAuthorizeUrl(conversationReference);

                reply.Text = Strings.VSTSLoginTitle;

                //Login Card
                var loginCard = new HeroCard
                {
                    Title = Strings.VSTSLoginCardTitle,
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, Strings.VSTSLoginCardButtonCaption, value: vstsLoginUrl) }
                };

                reply.Attachments.Add(loginCard.ToAttachment());

                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync(Strings.VSTSLoginSessionExistsPrompt);
                await context.PostAsync(Strings.VSTSlogoutPrompt);
                context.Done(token);
            }
        }
    }
}