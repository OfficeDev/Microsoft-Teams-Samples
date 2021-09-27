// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using FetchGroupChatMessagesWithRSC.helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FetchGroupChatMessagesWithRSC.Bots
{
    public class ActivityBot : TeamsActivityHandler
    {
        public readonly IConfiguration _configuration;
        private readonly GetChatHelper _helper = new();
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _clientFactory;

        public ActivityBot(IConfiguration configuration, IWebHostEnvironment env, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var activity = this.StripAtMentionText((Activity)turnContext.Activity);
            var userCommand = activity.Text;
            if(userCommand!= "getchat")
            {
                this.createChatFile(activity);
            }
            else
            {
                //var resource = await userTokenClient.GetSignInResourceAsync(_configuration["ConnectionName"], turnContext.Activity as Activity, null, cancellationToken);
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                //var signInLink = resource.SignInLink;
                var attachment = new Attachment
                {
                    Content = new OAuthCard
                    {
                        ConnectionName = _configuration["ConnectionName"],
                        Text = "Please login",
                        
                    },
                    ContentType = OAuthCard.ContentType,
                };
                var activiti = MessageFactory.Attachment(attachment);

                // NOTE: This activity needs to be sent in the 1:1 conversation between the bot and the user. 
                // If the bot supports group and channel scope, this code should be updated to send the request to the 1:1 chat. 

                await turnContext.SendActivityAsync(activiti, cancellationToken);
                //var chat = await _helper.GetGroupChatMessage(turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], turnContext.Activity.Conversation.Id);

                string filename = "chat.txt";
                string filePath = Path.Combine(_env.ContentRootPath, $".\\public\\chat.txt");
                long fileSize = new FileInfo(filePath).Length;
                await SendFileCardAsync(turnContext, filename, fileSize, cancellationToken); 
                await turnContext.SendActivityAsync(MessageFactory.Text("hello"));
            }
            return;
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync
    (ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            // Check the state value
            var state = turnContext.Activity.Value.ToString();
            var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            try
            {
                if (turnContext.Activity.Name == SignInConstants.TokenExchangeOperationName && turnContext.Activity.ChannelId == Channels.Msteams)
                {
                    await OnTokenResponseEventAsync((ITurnContext<IEventActivity>)turnContext, cancellationToken);
                    return new InvokeResponse() { Status = 200 };
                }
                else
                {
                    return await base.OnInvokeActivityAsync(turnContext, cancellationToken);
                }
            }
            catch (InvokeResponseException e)
            {
                return e.CreateInvokeResponse();
            }
        }

        /// <summary>
        /// Overriding to send welcome card once Bot/ME is installed in team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Welcome card  when bot is added first time by user.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome! With this sample your bot can fetch groupchat messages send it to the same chat as a file";
            foreach (var member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private async Task SendFileCardAsync(ITurnContext turnContext, string filename, long filesize, CancellationToken cancellationToken)
        {
            var consentContext = new Dictionary<string, string>
            {
                { "filename", filename },
            };

            var fileCard = new FileConsentCard
            {
                Description = "This is the file I want to send you",
                SizeInBytes = filesize,
                AcceptContext = consentContext,
                DeclineContext = consentContext,
            };

            var asAttachment = new Attachment
            {
                Content = fileCard,
                ContentType = FileConsentCard.ContentType,
                Name = filename,
            };

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments = new List<Attachment>() { asAttachment };
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        protected override async Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            try
            {
                JToken context = JObject.FromObject(fileConsentCardResponse.Context);

                string filePath = Path.Combine(_env.ContentRootPath, $".\\public\\chat.txt");
                long fileSize = new FileInfo(filePath).Length;
                var client = _clientFactory.CreateClient();
                using (var fileStream = File.OpenRead(filePath))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentLength = fileSize;
                    fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, fileSize - 1, fileSize);
                    await client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, fileContent, cancellationToken);
                }

                await FileUploadCompletedAsync(turnContext, fileConsentCardResponse, cancellationToken);
            }
            catch (Exception e)
            {
                await FileUploadFailedAsync(turnContext, e.ToString(), cancellationToken);
            }
        }

        protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            JToken context = JObject.FromObject(fileConsentCardResponse.Context);

            var reply = MessageFactory.Text($"Declined. We won't upload file <b>{context["filename"]}</b>.");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task FileUploadCompletedAsync(ITurnContext turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            var downloadCard = new FileInfoCard
            {
                UniqueId = fileConsentCardResponse.UploadInfo.UniqueId,
                FileType = fileConsentCardResponse.UploadInfo.FileType,
            };

            var asAttachment = new Attachment
            {
                Content = downloadCard,
                ContentType = FileInfoCard.ContentType,
                Name = fileConsentCardResponse.UploadInfo.Name,
                ContentUrl = fileConsentCardResponse.UploadInfo.ContentUrl,
            };

            var reply = MessageFactory.Text($"<b>File uploaded.</b> Your file <b>{fileConsentCardResponse.UploadInfo.Name}</b> is ready to download");
            reply.TextFormat = "xml";
            reply.Attachments = new List<Attachment> { asAttachment };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task FileUploadFailedAsync(ITurnContext turnContext, string error, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"<b>File upload failed.</b> Error: <pre>{error}</pre>");
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private Activity StripAtMentionText(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            foreach (var m in activity.GetMentions())
            {
                if (m.Mentioned.Id == activity.Recipient.Id)
                {
                    //Bot is in the @mention list.  
                    //The below example will strip the bot name out of the message, so you can parse it as if it wasn't included.
                    //Note that the Text object will contain the full bot name, if applicable.
                    if (m.Text != null)
                        activity.Text = activity.Text.Replace(m.Text, "").Trim();
                }
            }

            return activity;
        }

        private void createChatFile(Activity activity)
        {
            var fileName = Path.Combine(_env.ContentRootPath, $".\\public\\chat.txt");
            // string fileName = "C:\\Users\\v-nikija\\source\\repos\\Microsoft-Teams-Samples\\samples\\bot-groupchat-messages-withRSC\\csharp\\FetchGroupChatMessagesWithRSC\\public\\chat.txt";
            FileInfo fi = new FileInfo(fileName);
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (fi.Exists)
                {
                    using (StreamWriter sw = fi.AppendText())
                    {
                        sw.WriteLine("from: {0}", activity.From.Name);
                        sw.WriteLine("text: {0}", activity.Text);
                        sw.WriteLine("at: {0}", activity.LocalTimestamp);
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }

        }

        /// <summary>
        /// Get token response on basis of state.
        /// </summary>
        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext, string state, CancellationToken cancellationToken)
        {
            var magicCode = string.Empty;

            if (!string.IsNullOrEmpty(state))
            {
                if (int.TryParse(state, out var parsed))
                {
                    magicCode = parsed.ToString();
                }
            }

            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _configuration["ConnectionName"], turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }
    }
}