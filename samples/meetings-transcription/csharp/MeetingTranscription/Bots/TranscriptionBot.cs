// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using MeetingTranscription.Helpers;
using MeetingTranscription.Models.Configuration;
using MeetingTranscription.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscription.Bots
{
    public class TranscriptionBot : TeamsActivityHandler
    {
        /// <summary>
        /// Helper instance to make graph calls.
        /// </summary>
        private readonly GraphHelper graphHelper;

        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        private readonly ConcurrentDictionary<string, string> transcriptsDictionary;

        private readonly ICardFactory cardFactory;

        public TranscriptionBot(IOptions<AzureSettings> azureSettings, ConcurrentDictionary<string, string> transcriptsDictionary, ICardFactory cardFactory)
        {
            this.transcriptsDictionary = transcriptsDictionary;
            this.azureSettings = azureSettings;
            graphHelper = new GraphHelper(azureSettings);
            this.cardFactory = cardFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
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

        protected override async Task OnTeamsMeetingEndAsync(MeetingEndEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var meet = await TeamsInfo.GetMeetingInfoAsync(turnContext);

            var result = await graphHelper.GetMeetingTranscriptionsAsync(meet.Details.MsGraphResourceId, turnContext.Activity.From.AadObjectId);
            if (result != "Transcripts not found")
            {
                transcriptsDictionary.AddOrUpdate(meet.Details.MsGraphResourceId, result, (key, newValue) => result);

                var attachment = this.cardFactory.CreateAdaptiveCardAttachement(new { MeetingId = meet.Details.MsGraphResourceId });
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                var meetingId = JObject.FromObject(taskModuleRequest.Data)["meetingId"];

                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = $"{this.azureSettings.Value.AppBaseUrl}/home?meetingId={meetingId}",
                            Height = 600,
                            Width = 600,
                            Title = "Upload file",
                        },
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = this.azureSettings.Value.AppBaseUrl + "/home",
                            Height = 350,
                            Width = 350,
                            Title = "Upload file",
                        },
                    }
                };
            }
        }
    }
}