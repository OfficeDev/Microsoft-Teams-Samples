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
    public class VSTSGetworkItemDialog : IDialog<string>
    {

        /// <summary>
        /// The key that is used to keep the AccessToken in <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PrivateUserData"/>
        /// </summary>
        public static readonly string VSTSAuthTokenKey = ConfigurationManager.AppSettings["VSTSAuthToken"].ToString();

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Strings.VSTSGetWorkItemPrompt);
            context.Wait(this.MessageReceivedAsync);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);

            ConversationReference conversationReference;
            VSTSAcessToken vstsToken = new VSTSAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;
            string refreshToken = string.Empty;
            String error = null;
            string requestedWorkItemId = string.Empty;
            bool IsAuthenticated = false;

            if (context.UserData.TryGetValue("persistedCookieVSTS", out conversationReference))
            {
                requestedWorkItemId = conversationReference.User.Properties["workItemId"].ToString();

                if(string.IsNullOrEmpty(requestedWorkItemId))
                {
                    requestedWorkItemId = msg.Text;
                    IsAuthenticated = true;
                }
                else
                {
                    magicNumber = conversationReference.User.Properties["MagicNumber"].ToString();

                    if (string.Equals(msg.Text, magicNumber))
                    {
                        IsAuthenticated = true;
                    }
                    else
                    {
                        //When entered number is not valid
                        await context.PostAsync(Strings.AuthMagicNumberNotMacthed);
                        await LogIn(context, msg.Text);
                    }
                }

                if (IsAuthenticated)
                {
                    refreshToken = conversationReference.User.Properties["RefreshToken"].ToString();

                    //Get the refreshed token
                    error = VSTSHelpers.PerformTokenRequest(VSTSHelpers.GenerateRefreshPostData(refreshToken), true, out vstsToken);

                    if (String.IsNullOrEmpty(error))
                    {
                        conversationReference.User.Properties["AccessToken"] = vstsToken.accessToken;
                        conversationReference.User.Properties["RefreshToken"] = vstsToken.refreshToken;
                    }

                    WorkItem workItem = await VSTSHelpers.GetWorkItem(vstsToken.accessToken, requestedWorkItemId);

                    if (workItem != null)
                    {
                        var workItemCardMessage = CreateWorkItemCard(context, workItem, requestedWorkItemId);
                        await context.PostAsync(workItemCardMessage);
                    }
                }
            }
            else
            {
                await LogIn(context, msg.Text);
            }
        }

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(IDialogContext context, string workItemId)
        {
            string token;
            if (!context.UserData.TryGetValue(VSTSAuthTokenKey, out token))
            {
                var conversationReference = context.Activity.ToConversationReference();

                context.UserData.SetValue("persistedCookieVSTS", conversationReference);
                conversationReference.User.Properties.Add("workItemId", workItemId);

                // sending the sigin card with Facebook login url
                var reply = context.MakeMessage();
                var vstsLoginUrl = VSTSHelpers.GenerateAuthorizeUrl(conversationReference);

                reply.Text = Strings.VSTSGetWorkItemLoginPrompt;

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

        #region Create Work Item Message Card
        private Attachment CreateWorkItemAttachment(WorkItem workItem, string workItemId)
        {
            var encodedUrl = workItem.TeamProject;
            string goToWorkItemUrl = "https://teamsbot.visualstudio.com/" + encodedUrl + "/_workitems?id=" + workItemId + "&_a=edit";
            return new HeroCard
            {
                Title = workItem.Title,
                Subtitle = workItem.Url,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, Strings.VSTSGetWorkItemCardButtonCaption, value: goToWorkItemUrl),
                }
            }.ToAttachment();
        }

        private IMessageActivity CreateWorkItemCard(IDialogContext context, WorkItem workItem, string workItemId)
        {
            var message = context.MakeMessage();
            var attachment = CreateWorkItemAttachment(workItem, workItemId);
            message.Attachments.Add(attachment);
            return message;
        }
        #endregion

    }
}