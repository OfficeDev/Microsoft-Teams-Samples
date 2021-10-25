using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotWithSharePointFileViewer.helper
{
    public class GetSharePointFileHelper
    {
        // Get share point file.
        public static async Task<List<string>> GetSharePointFile(ITurnContext turnContext, TokenResponse tokenResponse, string sharepointSiteName, string sharepointTenantName)
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
                var fileNameList = await client.GetSharePointFile(sharepointSiteName, sharepointTenantName);

                return fileNameList;
            }
            catch (ServiceException ex)
            {
                throw ex;
            }
        }

        // Send archive messages file to user.
        public static async Task SendGroupChatMessage(ITurnContext turnContext, long fileSize,string microsoftAppId, string microsoftAppPassword,  CancellationToken cancellationToken)
        {

            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }
            try
            {
                string filename = "word.docx";
                var member = new ChannelAccount
                {
                    AadObjectId = turnContext.Activity.From.AadObjectId,
                    Name = turnContext.Activity.From.Name,
                    Id = turnContext.Activity.From.Id
                };

                ConversationReference conversationReference = null;

                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = new ChannelAccount[] { member },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

                var credentials = new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword);
                var serviceUrl = turnContext.Activity.ServiceUrl;

                // Creates a conversation on the specified groupchat and send file consent card on that conversation.
                await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                   turnContext.Activity.ChannelId, 
                   serviceUrl,
                   credentials,
                   conversationParameters,
                    async (conversationTurnContext, conversationCancellationToken) =>
                    {
                        conversationReference = conversationTurnContext.Activity.GetConversationReference();
                        await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                            microsoftAppId,
                            conversationReference,
                            async (conversationContext, conversationCancellation) =>
                            {
                                var replyActivity = SendFileCardAsync(turnContext, filename, fileSize);
                                await conversationContext.SendActivityAsync(replyActivity, conversationCancellation);
                            },
                            cancellationToken);
                    }, cancellationToken);
            }
            catch (ServiceException ex)
            {
                throw ex;
            }
        }

        // Send consent card to user.
        private static Activity SendFileCardAsync(ITurnContext turnContext, string filename, long filesize)
        {
            var consentContext = new Dictionary<string, string>
            {
                { "filename", filename },
            };

            var fileCard = new FileConsentCard
            {
                Description = "This is the archive chat file I want to send you",
                SizeInBytes = filesize,
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
            replyActivity.Attachments = new List<Microsoft.Bot.Schema.Attachment>() { asAttachment };
            return replyActivity;
        }
    }
}