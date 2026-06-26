// <copyright file="TranscriptionBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using MeetingTranscription.Helpers;
using MeetingTranscription.Models.Configuration;
using MeetingTranscription.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        /// <summary>
        /// Store details of meeting transcript.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> transcriptsDictionary;

        /// <summary>
        /// Instance of card factory to create adaptive cards.
        /// </summary>
        private readonly ICardFactory cardFactory;

        /// <summary>
        /// Creates bot instance.
        /// </summary>
        /// <param name="azureSettings">Stores the Azure configuration values.</param>
        /// <param name="transcriptsDictionary">Store details of meeting transcript.</param>
        /// <param name="cardFactory">Instance of card factory to create adaptive cards.</param>
        public TranscriptionBot(IOptions<AzureSettings> azureSettings, ConcurrentDictionary<string, string> transcriptsDictionary, ICardFactory cardFactory)
        {
            this.transcriptsDictionary = transcriptsDictionary;
            this.azureSettings = azureSettings;
            graphHelper = new GraphHelper(azureSettings);
            this.cardFactory = cardFactory;
        }

        /// <summary>
        /// Activity handler for on message activity.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        /// <summary>
        /// Activity handler for meeting end event.
        /// </summary>
        /// <param name="meeting">The details of the meeting.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsMeetingEndAsync(MeetingEndEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext);
                Console.WriteLine($"Meeting Ended: {meetingInfo.Details.MsGraphResourceId}");

                // NEW: Get meeting organizer information when meeting ends
                var organizerId = await graphHelper.GetMeetingOrganizerFromTeamsContextAsync(turnContext);
                if (!string.IsNullOrEmpty(organizerId))
                {
                    Console.WriteLine($"Meeting organizer identified: {organizerId}");
                }

                // NEW: Use Teams context to find organizer and get transcripts
                var result = await graphHelper.GetMeetingTranscriptionsAsync(meetingInfo.Details.MsGraphResourceId, organizerId);
                
                if (!string.IsNullOrEmpty(result))
                {
                    transcriptsDictionary.AddOrUpdate(meetingInfo.Details.MsGraphResourceId, result, (key, newValue) => result);

                    var attachment = this.cardFactory.CreateAdaptiveCardAttachement(new { MeetingId = meetingInfo.Details.MsGraphResourceId });
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                    
                    Console.WriteLine($"Successfully retrieved and cached meeting transcript for {meetingInfo.Details.MsGraphResourceId}");
                }
                else
                {
                    var attachment = this.cardFactory.CreateNotFoundCardAttachement();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                    
                    Console.WriteLine($"No transcript found for meeting {meetingInfo.Details.MsGraphResourceId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnTeamsMeetingEndAsync: {ex.Message}");
                
                // Send error card to user
                var errorAttachment = this.cardFactory.CreateNotFoundCardAttachement();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(errorAttachment), cancellationToken);
            }
        }

        /// <summary>
        /// Activity handler for Task module fetch event.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
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
                            Title = "Meeting Transcript",
                        },
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnTeamsTaskModuleFetchAsync: {ex.Message}");

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
                            Title = "Meeting Transcript",
                        },
                    }
                };
            }
        }
    }
}