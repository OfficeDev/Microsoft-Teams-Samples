// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using MeetingExtension_SP.Handlers;
using MeetingExtension_SP.Helpers;
using MeetingExtension_SP.Models;
using MessageExtension_SP.Handlers;
using MessageExtension_SP.Helpers;
using MessageExtension_SP.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingExtension_SP.Bots
{
    /// <summary>
    /// ShsrepointBot with MessageExtension with Search and Action
    /// </summary>
    public class SharePointBot : TeamsActivityHandler
    {
        private readonly IConfiguration configuration;

        public SharePointBot(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "composeExtension/fetchTask")
            { 
                MessagingExtensionAction action = new MessagingExtensionAction();   
                var task = await this.OnTeamsMessagingExtensionFetchTaskAsync(turnContext, action, cancellationToken);
                return CreateInvokeResponse(task);
            }
            if (turnContext.Activity.Name == "composeExtension/query")
            {
                var quer = JsonConvert.DeserializeObject<MESearch>(turnContext.Activity.Value.ToString());
                MessagingExtensionQuery query = new MessagingExtensionQuery();
                query.Parameters = quer.Parameters;
                query.CommandId = quer.commandId;
                var result = await this.OnTeamsMessagingExtensionQueryAsync(turnContext, query, cancellationToken);
                return CreateInvokeResponse(result);
            }
            else if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                var data = JsonConvert.DeserializeObject<ActionType>(turnContext.Activity.Value.ToString());

                string cases = data.action.verb.ToString();
                SPFileHandler handler = new SPFileHandler();

                switch (cases.ToString())
                {
                    case "approveRequest":
                        //Approve the request 
                        await handler.ApproveFileAsync(configuration);
                        Microsoft.Bot.Schema.Attachment card = CardHelper.CreateAdaptiveCardAttachment(MessageExtension_SP.Helpers.Constants.ApprovedCard, configuration);
                        var activity = MessageFactory.Attachment(card);
                        activity.Id = turnContext.Activity.ReplyToId;
                        await turnContext.UpdateActivityAsync(activity, cancellationToken);
                        break;
                    case "rejectRequest":
                        // reject the request                        
                        await handler.RejectFileAsync(configuration); 
                        Microsoft.Bot.Schema.Attachment card1 = CardHelper.CreateAdaptiveCardAttachment(MessageExtension_SP.Helpers.Constants.RejectedCard, configuration);
                        var activities = MessageFactory.Attachment(card1);
                        activities.Id = turnContext.Activity.ReplyToId;
                        await turnContext.UpdateActivityAsync(activities, cancellationToken);
                        break;

                    case "cardRefresh":
                        //read conversation id from text file
                        string conversationId = System.IO.File.ReadAllText(@"Temp/ConversationFile.txt");
                        if (turnContext.Activity.Conversation.Id == conversationId)
                        {
                            string userId = turnContext.Activity.From.AadObjectId;
                            string teamOwnerId = await Common.GetManagerId(configuration);
                            //based on owner id show the approver card
                            if (userId == teamOwnerId)
                            {
                                return GetStatusCard(MessageExtension_SP.Helpers.Constants.OwnerCard);
                            }
                        }
                        return GetStatusCard(null);
                }

            }

            return GetStatusCard(null);
        }
      
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text?.ToLower().TrimEnd().TrimStart();
            var reply = MessageFactory.Text(string.Empty);
            if (text != null)
            {
                var replyText = $"SPShareDoc : {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var title = string.Empty;
            var data = turnContext.Activity.Value;
            var titleParam = query.Parameters?.FirstOrDefault(p => (p.Name == "Documents" || p.Name == "assetTitle"));
            if (titleParam != null)
            {
                title = titleParam.Value.ToString();
            }

            var assets = await MESearchHandler.Search(query.CommandId, title, this.configuration);

            if (assets.Count == 0)
            {
                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "message",
                        Text = "No match found.",
                    },
                };
            }

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
            var attachments = assets.Select(asset =>
            {
                var previewCard = new ThumbnailCard
                {
                    Title = asset.Title
                };
                previewCard.Images = new List<CardImage>() { new CardImage(configuration["BaseUri"] + "/Images/MSC17_cloud_006.png") };
                CardHelper cardhelper = new CardHelper();
                var assetCard = cardhelper.GetAssetCard(asset);
                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = AdaptiveCards.AdaptiveCard.ContentType,
                    Content = assetCard.Content,
                    Preview = previewCard.ToAttachment(),
                };

                return attachment;
            }).ToList();

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments,
                },
            };

            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            //get and store the user details in temp file
            var tempFilePath = @"Temp/UserFile.txt";
            System.IO.File.WriteAllText(tempFilePath, turnContext.Activity.From.Name + "," + turnContext.Activity.From.AadObjectId + "," + turnContext.Activity.From.Id);

            var response = new MessagingExtensionActionResponse()
                {
                    Task = new TaskModuleContinueResponse()
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Height = 400,
                            Width = 600,
                            //Title = "Invite people to share how they feel",
                            Url = this.configuration["BaseUri"] + "/FileUpload/Upload"
                        },
                    },
                };

                return response;
            
        }


        private InvokeResponse GetStatusCard(string cardType)
        {
            if (!string.IsNullOrEmpty(cardType))
            {
                Microsoft.Bot.Schema.Attachment card2 = CardHelper.CreateAdaptiveCardAttachment(cardType, configuration);

                InvokeResponseBody response2 = new InvokeResponseBody();
                response2.statusCode = 200;
                response2.type = "application/vnd.microsoft.card.adaptive";
                response2.value = card2.Content;
                InvokeResponse invokeResponse2 = new InvokeResponse();
                invokeResponse2.Body = response2;
                invokeResponse2.Status = 200;
                return invokeResponse2;
            }
            else
            {
                InvokeResponseBody response2 = new InvokeResponseBody();
                response2.statusCode = 200;
                response2.type = "application/vnd.microsoft.activity.message";
                response2.value = "message";
                InvokeResponse invokeResponse2 = new InvokeResponse();
                invokeResponse2.Body = response2;
                invokeResponse2.Status = 200;
                return invokeResponse2;
            }
        }
    }
}
