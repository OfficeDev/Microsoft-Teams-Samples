using Autofac;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
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
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string botId, [FromUri] string conversationId, [FromUri] string channelId, [FromUri] string serviceUrl, [FromUri] string code, [FromUri] string state, CancellationToken token)
        {
            // Get the resumption cookie
            var address = new Address
                (
                    // purposefully using named arguments because these all have the same type
                    botId: FacebookHelpers.TokenDecoder(botId),
                    channelId: channelId,
                    userId: FacebookHelpers.TokenDecoder(userId),
                    conversationId: FacebookHelpers.TokenDecoder(conversationId),
                    serviceUrl: FacebookHelpers.TokenDecoder(serviceUrl)
                );

            var conversationReference = address.ToConversationReference();

            // Exchange the Facebook Auth code with Access token
            var accessToken = await FacebookHelpers.ExchangeCodeForAccessToken(conversationReference, code, SimpleFacebookAuthDialog.FacebookOauthCallback.ToString());

            //Set the User Token, Magic Number and IsValidated Property to User Properties.
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBAccessTokenKey"].ToString(), accessToken.AccessToken);
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBMagicNumberKey"].ToString(), ConfigurationManager.AppSettings["FBMagicNumberValue"].ToString());
            conversationReference.User.Properties.Add(ConfigurationManager.AppSettings["FBIsValidatedKey"].ToString(), false);

            // Create the message that is send to conversation to resume the login flow
            var msg = conversationReference.GetPostToBotMessage();
            msg.Text = $"token:{accessToken.AccessToken}";

            // Resume the conversation to SimpleFacebookAuthDialog
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, msg))
            {
                var dataBag = scope.Resolve<IBotData>();
                await dataBag.LoadAsync(token);

                ConversationReference pending;
                var connector = new ConnectorClient(new Uri(conversationReference.ServiceUrl));

                if (dataBag.PrivateConversationData.TryGetValue("persistedCookie", out pending))
                {
                    dataBag.PrivateConversationData.SetValue("persistedCookie", conversationReference);

                    await dataBag.FlushAsync(token);

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
}
