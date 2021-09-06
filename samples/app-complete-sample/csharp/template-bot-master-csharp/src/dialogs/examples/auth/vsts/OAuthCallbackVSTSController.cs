using Autofac;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Teams.TemplateBotCSharp
{
    public class OAuthCallbackVSTSController : ApiController
    {
        protected readonly IStatePropertyAccessor<PrivateConversationData> _privateCoversationState;
        public OAuthCallbackVSTSController(PrivateConversationState privateCoversationState)
        {
            _privateCoversationState = privateCoversationState.CreateProperty<PrivateConversationData>(nameof(PrivateConversationData));
        }
        /// <summary>
        /// OAuth call back that is called by VSTS. Read https://www.visualstudio.com/en-us/docs/integrate/extensions/overview for more details.
        /// </summary>
        /// <param name="code"> The Authentication code returned by VSTS.</param>
        /// <param name="state"> The state returned by VSTS.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/OAuthCallbackVSTS")]
        public async Task<HttpResponseMessage> OAuthCallbackVSTS(string code, string state, CancellationToken token)
        {
            ConversationReference conversationReferenceJSON = JsonConvert.DeserializeObject<ConversationReference>(state);

            var conversationReference = conversationReferenceJSON;

            string workItemId = string.Empty;

            if (conversationReferenceJSON.User.Properties["workItemId"] != null)
            {
                workItemId = conversationReferenceJSON.User.Properties["workItemId"].ToString();
                conversationReference.User.Properties.Add("workItemId", workItemId);
            }

            VSTSAcessToken accessToken = new VSTSAcessToken();
            String error = null;

            if (!String.IsNullOrEmpty(code))
            {
                error = VSTSHelpers.PerformTokenRequest(VSTSHelpers.GenerateRequestPostData(code), true, out accessToken);

                //Set the User Token, Magic Number and IsValidated Property to User Properties.    
                if (String.IsNullOrEmpty(error))
                {
                    conversationReference.User.Properties.Add("AccessToken", accessToken.accessToken);
                    conversationReference.User.Properties.Add("RefreshToken", accessToken.refreshToken);
                    conversationReference.User.Properties.Add("MagicNumber", ConfigurationManager.AppSettings["VSTSMagicNumber"].ToString());
                    conversationReference.User.Properties.Add("IsValidated", false);
                    conversationReference.User.Properties.Add("UserName", conversationReferenceJSON.User.Name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException(Strings.VSTSCallbackAuthError));
            }

            // Create the message that is send to conversation to resume the login flow
            var msg = conversationReference.GetContinuationActivity();
            msg.Text = $"token:{accessToken.accessToken}";

            // Resume the conversation to SimpleFacebookAuthDialog
            var currentState = await this._privateCoversationState.GetAsync(turnContext, () => new PrivateConversationData());
            ConversationReference pending = currentState.PersistedCookieVSTS;
            var connector = new ConnectorClient(new Uri(conversationReference.ServiceUrl));

            if (pending != null)
            {
                currentState.PersistedCookieVSTS = conversationReference;
                await this._privateCoversationState.SetAsync(turnContext, currentState);

                //Send message to Bot
                IMessageActivity message = Activity.CreateMessageActivity();
                message.From = conversationReference.User;
                message.Recipient = conversationReference.User;
                message.Conversation = new ConversationAccount(id: conversationReference.Conversation.Id);
                message.Text = Strings.OAuthCallbackUserPrompt;
                await connector.Conversations.SendToConversationAsync((Activity)message);

                return Request.CreateResponse(Strings.OAuthCallbackMessage);
            }
            else
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException(Strings.AuthCallbackResumeError));
            }
        }
    }
}