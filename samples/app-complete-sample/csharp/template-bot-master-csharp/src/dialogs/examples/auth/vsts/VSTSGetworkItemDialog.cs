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
    public class VSTSGetworkItemDialog : ComponentDialog
    {
        /// <summary>
        /// The key that is used to keep the AccessToken in <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PrivateUserData"/>
        /// </summary>
        public static readonly string VSTSAuthTokenKey = ConfigurationManager.AppSettings["VSTSAuthToken"].ToString();
        protected readonly IStatePropertyAccessor<PrivateConversationData> _privateCoversationState;
        public VSTSGetworkItemDialog(IStatePropertyAccessor<PrivateConversationData> privateCoversationState) : base(nameof(SimpleFacebookAuthDialog))
        {
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
            await stepContext.Context.SendActivityAsync(Strings.VSTSGetWorkItemPrompt);
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
            ConversationReference conversationReference;
            VSTSAcessToken vstsToken = new VSTSAcessToken();
            string magicNumber = string.Empty;
            string token = string.Empty;
            string refreshToken = string.Empty;
            String error = null;
            string requestedWorkItemId = string.Empty;
            bool IsAuthenticated = false;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(stepContext.Context, () => new PrivateConversationData());
            conversationReference = currentPrivateConversationData.PersistedCookieVSTS;
            if (currentPrivateConversationData.PersistedCookieVSTS!=null)
            {
                requestedWorkItemId = conversationReference.User.Properties["workItemId"].ToString();

                if (string.IsNullOrEmpty(requestedWorkItemId))
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
                        await stepContext.Context.SendActivityAsync(Strings.AuthMagicNumberNotMacthed);
                        await LogIn(stepContext, msg.Text);
                        return await stepContext.ContinueDialogAsync();
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
                        var workItemCardMessage = CreateWorkItemCard(stepContext, workItem, requestedWorkItemId);
                        await stepContext.Context.SendActivityAsync(workItemCardMessage);
                    }
                    return await stepContext.ContinueDialogAsync();
                }
                return await stepContext.EndDialogAsync();
            }
            else
            {
                await LogIn(stepContext, msg.Text);
                return await stepContext.ContinueDialogAsync();
            }
        }

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <param name="context"> The Dialog context.</param>
        /// <returns> A task that represents the login action.</returns>
        private async Task LogIn(WaterfallStepContext context, string workItemId)
        {
            string token;
            var currentPrivateConversationData = await this._privateCoversationState.GetAsync(context.Context, () => new PrivateConversationData());
            token = currentPrivateConversationData.VSTSAuthTokenKey;
            if (currentPrivateConversationData.VSTSAuthTokenKey != null)
            {
                var conversationReference = context.Context.Activity.GetConversationReference();

                currentPrivateConversationData.PersistedCookieVSTS = conversationReference;
                await this._privateCoversationState.SetAsync(context.Context, currentPrivateConversationData);
                conversationReference.User.Properties.Add("workItemId", workItemId);

                // sending the sigin card with Facebook login url
                var reply = context.Context.Activity;
                var vstsLoginUrl = VSTSHelpers.GenerateAuthorizeUrl(conversationReference);

                reply.Text = Strings.VSTSGetWorkItemLoginPrompt;

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

        private IMessageActivity CreateWorkItemCard(WaterfallStepContext context, WorkItem workItem, string workItemId)
        {
            var message = context.Context.Activity;
            var attachment = CreateWorkItemAttachment(workItem, workItemId);
            message.Attachments = new List<Attachment> { attachment };
            return message;
        }
        #endregion

    }
}