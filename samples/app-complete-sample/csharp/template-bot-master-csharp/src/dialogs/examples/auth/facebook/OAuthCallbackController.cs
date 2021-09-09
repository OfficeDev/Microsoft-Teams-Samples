using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Teams.TemplateBotCSharp
{
    public class OAuthCallbackController : ApiController
    {
        /// <summary>
        /// Bot adapter.
        /// </summary>
        private readonly BotFrameworkHttpAdapter botAdapter;

        protected readonly IStatePropertyAccessor<PrivateConversationData> _privateCoversationState;
        public OAuthCallbackController(PrivateConversationState privateCoversationState, BotFrameworkHttpAdapter adapter)
        {
            _privateCoversationState = privateCoversationState.CreateProperty<PrivateConversationData>(nameof(PrivateConversationData));
            botAdapter = adapter;
        }
        /// <summary>
        /// Facebook OAuth Callback Method
        /// </summary>
        /// <param name="userId">The Id for the user that is getting authenticated.</param>
        /// <param name="botId">Bot Id.</param>
        /// <param name="conversationId">The Id of the conversation.</param>
        /// <param name="channelId">The Id of the channel.</param>
        /// <param name="serviceUrl">The Id of the Teams Service url.</param>
        /// <param name="code">The Authentication code returned by Facebook.</param>
        /// <param name="state">The state returned by Facebook.</param>        
        /// <returns></returns>
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string botId, [FromUri] string conversationId, [FromUri] string serviceUrl, [FromUri] string channelId, [FromUri] string code, [FromUri] string state, CancellationToken token)
        {
            ConversationReference conversationReferenceJSON = new ConversationReference();
            conversationReferenceJSON.Bot = new ChannelAccount()
            {
                Id = FacebookHelpers.TokenDecoder(botId)
            };
            conversationReferenceJSON.User = new ChannelAccount()
            {
                Id = FacebookHelpers.TokenDecoder(userId)
            };
            conversationReferenceJSON.Conversation = new ConversationAccount()
            {
                Id = FacebookHelpers.TokenDecoder(conversationId)
            };
            conversationReferenceJSON.ChannelId = channelId;
            conversationReferenceJSON.ServiceUrl = FacebookHelpers.TokenDecoder(serviceUrl);

            // Get the resumption cookie
            var conversationReference = conversationReferenceJSON;

            // Exchange the Facebook Auth code with Access token
            var accessToken = await FacebookHelpers.ExchangeCodeForAccessToken(conversationReference, code, SimpleFacebookAuthDialog.FacebookOauthCallback.ToString());

            //Set the User Token, Magic Number and IsValidated Property to User Properties.
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBAccessTokenKey"].ToString(), accessToken.AccessToken);
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBMagicNumberKey"].ToString(), ConfigurationManager.AppSettings["FBMagicNumberValue"].ToString());
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBIsValidatedKey"].ToString(), false);

            // Create the message that is send to conversation to resume the login flow
            var msg = conversationReference.GetContinuationActivity();
            msg.Text = $"token:{accessToken.AccessToken}";

            bool result = false;
            await ((BotFrameworkAdapter)this.botAdapter).ContinueConversationAsync(ConfigurationManager.AppSettings["MicrosoftAppPassword"], conversationReference, async (conversationTurnContext, conversationCancellationToken) =>
             {
                 var currentState = await this._privateCoversationState.GetAsync(conversationTurnContext, () => new PrivateConversationData());
                 ConversationReference pending = currentState.PersistedCookie;
                 if (pending != null)
                 {
                     currentState.PersistedCookie = conversationReference;
                     await this._privateCoversationState.SetAsync(conversationTurnContext, currentState);

                     //Send message to Bot
                     IMessageActivity message = Activity.CreateMessageActivity();
                     message.From = conversationReference.User;
                     message.Recipient = conversationReference.User;
                     message.Conversation = new ConversationAccount(id: conversationReference.Conversation.Id);
                     message.Text = Strings.OAuthCallbackUserPrompt;
                     await conversationTurnContext.SendActivityAsync((Activity)message);
                     result = true;
                 }
             }, CancellationToken.None);

            if (result == true)
            {
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