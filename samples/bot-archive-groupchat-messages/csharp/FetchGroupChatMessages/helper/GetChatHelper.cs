using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FetchGroupChatMessagesWithRSC.helper
{
    /// <summary>
    /// Helper class for fetching and sending group chat messages.
    /// </summary>
    public class GetChatHelper
    {
        /// <summary>
        /// Gets group chat messages.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="tokenResponse">The token response.</param>
        /// <param name="chatId">The chat ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat messages collection.</returns>
        public static async Task<IChatMessagesCollectionPage> GetGroupChatMessage(ITurnContext turnContext, TokenResponse tokenResponse, string chatId)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            try
            {
                var client = new SimpleGraphClient(tokenResponse.Token);
                var messages = await client.GetGroupChatMessages(chatId);
                return messages;
            }
            catch (ServiceException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends archive messages file to the user.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="fileSize">The file size.</param>
        /// <param name="microsoftAppId">The Microsoft app ID.</param>
        /// <param name="microsoftAppPassword">The Microsoft app password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendGroupChatMessage(ITurnContext turnContext, long fileSize, string microsoftAppId, string microsoftAppPassword, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            try
            {
                string filename = "chat.txt";
                var member = new ChannelAccount
                {
                    AadObjectId = turnContext.Activity.From.AadObjectId,
                    Name = turnContext.Activity.From.Name,
                    Id = turnContext.Activity.From.Id
                };

                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = new[] { member },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

                var credentials = new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword);
                var serviceUrl = turnContext.Activity.ServiceUrl;

                // Creates a conversation on the specified group chat and sends a file consent card in that conversation.
                await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                    turnContext.Activity.ChannelId,
                    serviceUrl,
                    credentials,
                    conversationParameters,
                    async (conversationTurnContext, conversationCancellationToken) =>
                    {
                        var conversationReference = conversationTurnContext.Activity.GetConversationReference();
                        await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                            microsoftAppId,
                            conversationReference,
                            async (conversationContext, conversationCancellation) =>
                            {
                                var replyActivity = CreateFileConsentCard(turnContext, filename, fileSize);
                                await conversationContext.SendActivityAsync(replyActivity, conversationCancellation);
                            },
                            cancellationToken);
                    }, cancellationToken);
            }
            catch (ServiceException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a file consent card activity.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="fileSize">The file size.</param>
        /// <returns>The activity with the file consent card.</returns>
        private static Activity CreateFileConsentCard(ITurnContext turnContext, string filename, long fileSize)
        {
            var consentContext = new Dictionary<string, string>
                {
                    { "filename", filename },
                };

            var fileCard = new FileConsentCard
            {
                Description = "This is the archive chat file I want to send you",
                SizeInBytes = fileSize,
                AcceptContext = consentContext,
                DeclineContext = consentContext,
            };

            var asAttachment = new Microsoft.Bot.Schema.Attachment
            {
                Content = fileCard,
                ContentType = FileConsentCard.ContentType,
                Name = filename,
            };

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments = new List<Microsoft.Bot.Schema.Attachment> { asAttachment };
            return replyActivity;
        }
    }
}