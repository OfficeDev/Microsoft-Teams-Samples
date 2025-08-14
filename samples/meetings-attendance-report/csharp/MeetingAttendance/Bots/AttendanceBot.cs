// <copyright file="AttendanceBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using MeetingAttendance.Helpers;
using MeetingAttendance.Models.Configuration;
using MeetingAttendance.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingAttendance.Bots
{
    public class AttendanceBot : TeamsActivityHandler
    {
        /// <summary>
        /// Helper instance to make graph calls.
        /// </summary>
        private readonly GraphHelper graphHelper;

        /// <summary>
        /// Creates bot instance.
        /// </summary>
        /// <param name="azureSettings">Stores the Azure configuration values.</param>
        /// <param name="graphClient">Instance of graph client to make Graph calls.</param>
        public AttendanceBot(IOptions<AzureSettings> azureSettings, GraphClient graphClient)
        {
            graphHelper = new GraphHelper(azureSettings, graphClient);
        }

        /// <summary>
        /// Activity handler invoked when bot is installed.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnInstallationUpdateAddAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CardFactory.GetWelcomeCard()), cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                await turnContext.SendActivityAsync(MessageFactory.Text("Please install this bot in meeting chat to get meeting attendance report"), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await turnContext.SendActivityAsync(MessageFactory.Text("Something went wrong! Please try installing bot again"), cancellationToken);
            }
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
            await Task.Delay(5000);
            var meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext);

            var result = await graphHelper.GetNewestMeetingAttendanceRecordsAsync(meetingInfo.Details.MsGraphResourceId);
            if (result != null)
            {
                var attachment = CardFactory.GetMeetingReportAdaptiveCard(result);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
        }
    }
}