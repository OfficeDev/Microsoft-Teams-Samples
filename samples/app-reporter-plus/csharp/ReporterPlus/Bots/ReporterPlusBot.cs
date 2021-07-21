using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReporterPlus.Models;
using ReporterPlus.Helpers;

namespace ReporterPlus.Bots
{
    public class ReporterPlusBot : TeamsActivityHandler
    {
        public ReporterPlusBot(IConfiguration configuration) : base()
        {
            Constants.MicrosoftAppId = configuration["MicrosoftAppId"];
            Constants.MicrosoftAppPassword = configuration["MicrosoftAppPassword"];
            Constants.TenantId = configuration["TenantId"];
            Constants.BaseUrl = configuration["BaseUrl"];
            Constants.ServiceUrl = configuration["ServiceUrl"];
            Constants.OriginatorId = configuration["OriginatorId"];
            Constants.SenderEmail = configuration["SenderEmail"];
            Constants.BlobContainerName = configuration["BlobContainerName"];
            Constants.BlobConnectionString = configuration["BlobConnectionString"];
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (Constants.ComposeExtensionFetch.Equals(turnContext.Activity.Name))
            {
                MessagingExtensionAction action = new MessagingExtensionAction();
                var actionJson = JsonConvert.DeserializeObject<MessagingExtensionActionDeserializer>(turnContext.Activity.Value.ToString());
                action.CommandId = actionJson.commandId;
                var task = await this.OnTeamsMessagingExtensionFetchTaskAsync(turnContext, action, cancellationToken);
                return CreateInvokeResponse(task);
            }
            else if (Constants.ComposeExtensionSubmit.Equals(turnContext.Activity.Name))
            {
                MessagingExtensionAction action = new MessagingExtensionAction();
                var task = await this.OnTeamsMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken);
                return CreateInvokeResponse(task);
            }
            else if (Constants.TaskModuleFetch.Equals(turnContext.Activity.Name))
            {
                var actionJson = JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());
                var task = await ReturnViewDetails(actionJson.data.reqId);
                return CreateInvokeResponse(task);
            }
            else if (Constants.AdaptiveCardAction.Equals(turnContext.Activity.Name))
            {
                var data = JsonConvert.DeserializeObject<ActionType>(turnContext.Activity.Value.ToString());
                string channel = turnContext.Activity.ChannelId;
                string verb = data.action.verb;
                var blobInfo = BlobHelper.GetBlob(data.action.data.reqId, verb).Result;
                string cardJsonString;

                if (Status.Refresh.Equals(verb) && Status.Pending.Equals(blobInfo.status))
                {
                    CardHelper.CreateAdaptiveCardAttachment(blobInfo.status, blobInfo, channel, out cardJsonString);
                    var cardResponse = JObject.Parse(cardJsonString);
                    var res = new AdaptiveCardInvokeResponse()
                    {
                        StatusCode = 200,
                        Type = AdaptiveCard.ContentType,
                        Value = cardResponse
                    };
                    return CreateInvokeResponse(res);
                }
                else
                {
                    var adaptiveCardAttachment = CardHelper.CreateAdaptiveCardAttachment(blobInfo.status, blobInfo, channel, out cardJsonString);
                    UpdateCardInTeams(blobInfo, adaptiveCardAttachment);
                    var cardResponse = JObject.Parse(cardJsonString);
                    var res = new AdaptiveCardInvokeResponse()
                    {
                        StatusCode = 200,
                        Type = AdaptiveCard.ContentType,
                        Value = cardResponse
                    };
                    return CreateInvokeResponse(res);
                }
            }
            return null;
        }

        private async void UpdateCardInTeams(BlobDataDeserializer blobData, Attachment adaptiveCardAttachment)
        {
            using var connector = new ConnectorClient(new Uri(Constants.ServiceUrl), Constants.MicrosoftAppId, Constants.MicrosoftAppPassword);
            AppCredentials.TrustServiceUrl(Constants.ServiceUrl, DateTime.MaxValue);
            var updateActivity = new Activity();
            updateActivity.Id = blobData.messageId;
            updateActivity.Type = "message";
            updateActivity.Attachments = new List<Attachment> { adaptiveCardAttachment };
            updateActivity.Conversation = new ConversationAccount(id: blobData.conversationId);
            await connector.Conversations.UpdateActivityAsync(updateActivity);
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var taskModuleOutput = JsonConvert.DeserializeObject<TaskModuleSubmitDataDeserializer>(turnContext.Activity.Value.ToString());
            if (taskModuleOutput.data.SubmittedByMail == null)
            {
                action.CommandId = taskModuleOutput.commandId;
                var task = await OnTeamsMessagingExtensionFetchTaskAsync(turnContext, action, cancellationToken);
                return task;
            }

            if (taskModuleOutput.commandId == Constants.MessageExtensionCommandId)
            {
                var member = await TeamsInfo.GetMemberAsync(turnContext, taskModuleOutput.data.AssignedTo.objectId, cancellationToken);
                taskModuleOutput.data.AssignedTo.objectId = member.Id;
                var blobId = await BlobHelper.UploadToBlob(taskModuleOutput, turnContext);
                var blobData = BlobHelper.GetBlob(blobId, null).Result;
                var cardResponse = CardHelper.CreateAdaptiveCardAttachment(Status.BaseCard, blobData, "msteams", out string cardJsonstring);
                var messageResponse =  await turnContext.SendActivityAsync(MessageFactory.Attachment(cardResponse), cancellationToken);
                string messageId = messageResponse.Id;
                BlobHelper.GetBlob(blobId, null, messageId);

                //Send Mail
                try
                {
                    await OutlookConnector.SendMailAsync(Constants.SenderEmail, blobData.assignedToMail, cardJsonstring, Constants.MailSubject);
                } catch (Exception ex)
                {
                    // Email sending would fail untill your request to send outlook actionable messages is approved.
                    Console.WriteLine("Failed to send email");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return new MessagingExtensionActionResponse();
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            string memberName;
            try
            {
                // Check if your app is installed by fetching member information.
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                memberName = member.Name;
            }
            catch (ErrorResponseException ex)
            {
                if (ex.Body.Error.Code == "BotNotInConversationRoster")
                {
                    return new MessagingExtensionActionResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo
                            {
                                Card = CardHelper.CreateJustInTimeCard("JITinstallation.json"),
                                Height = 200,
                                Width = 400,
                                Title = "Device Capabilities - App Installation",
                            },
                        },
                    };
                }
                throw;
            }

            if (action.CommandId == Constants.MessageExtensionCommandId)
            {
                var response = new MessagingExtensionActionResponse()
                {
                    Task = new TaskModuleContinueResponse()
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Height = 450,
                            Width = 350,
                            Title = "Bar Code Scanner",
                            Url = Constants.BaseUrl + "/Scanner",
                        },
                    },
                };
                return response;
            }
            return new MessagingExtensionActionResponse();
        }

        public async Task<TaskModuleResponse> ReturnViewDetails(string requestID)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = Constants.BaseUrl + "/ViewDetails?id=" + requestID,
                        FallbackUrl = Constants.BaseUrl + "/ViewDetails?id=" + requestID,
                        Height = 450,
                        Width = 350,
                        Title = "Details",
                    },
                },
            };
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Hi *{turnContext.Activity.From.Name}*, This is a ReporterPlus Application where you can make use of the features like *Barcode Scanner, Image Capturing, Audio Recording*, etc.";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to the ReporterPlus App";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
