// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingAttendance.Helpers
{
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using MeetingAttendance.Models.Configuration;
    using System;
    using System.Threading.Tasks;
    using MeetingAttendance.Models;
    using MeetingAttendance.Services;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Graph.SecurityNamespace;

    public class GraphHelper
    {
        /// <summary>
        /// Stores the Azure configuration values.
        /// </summary>
        private readonly IOptions<AzureSettings> azureSettings;

        /// <summary>
        /// Client to call graph APIs.
        /// </summary>
        private readonly GraphClient graphClient;

        public GraphHelper(IOptions<AzureSettings> azureSettings, GraphClient graphClient)
        {
            this.azureSettings = azureSettings;
            this.graphClient = graphClient;
        }

        /// <summary>
        /// Gets application token.
        /// </summary>
        /// <returns>Application token.</returns>
        public async Task<string> GetToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(this.azureSettings.Value.MicrosoftAppId)
                .WithClientSecret(this.azureSettings.Value.MicrosoftAppPassword)
                .WithAuthority($"https://login.microsoftonline.com/{this.azureSettings.Value.MicrosoftAppTenantId}")
                .WithRedirectUri("https://daemon")
                .Build();

            // TeamsAppInstallation.ReadWriteForChat.All Chat.Create User.Read.All TeamsAppInstallation.ReadWriteForChat.All
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        /// <summary>
        /// Gets the newest meeting attendance record for the passed meeting Id.
        /// </summary>
        /// <param name="meetingId">Id of meeting.</param>
        /// <returns>Meeting attendance record if any, otherwise return null.</returns>
        public async Task<MeetingAttendanceSummary> GetNewestMeetingAttendanceRecordsAsync(string meetingId)
        {
            try
            {
                var graphClient = this.graphClient.GetGraphClientforApp();
                var attendanceReports = await graphClient.Users[this.azureSettings.Value.UserId].OnlineMeetings[meetingId]
                    .AttendanceReports
                    .Request()
                    .GetAsync();

                if (attendanceReports.CurrentPage.Count > 0)
                {
                    var currentAttendanceReport = attendanceReports.CurrentPage[0];

                    var meetingTimeSpan = (currentAttendanceReport.MeetingEndDateTime - currentAttendanceReport.MeetingStartDateTime)?.TotalSeconds;

                    var meetingAttendanceReport = new MeetingAttendanceSummary
                    {
                        AttendaceReportId = currentAttendanceReport.Id,
                        MeetingDuration = GetDurationFormattedString((int)meetingTimeSpan),
                        MeetingStartAndEndTime = GetStartEndTimeFormattedStringForAdaptiveCard(currentAttendanceReport.MeetingStartDateTime, currentAttendanceReport.MeetingEndDateTime),
                        ParticipantCount = (int)currentAttendanceReport.TotalParticipantCount,
                        ParticipantsInfo = new List<MeetingParticipantRecord>(),
                    };

                    var attendanceRecords = await graphClient.Users[this.azureSettings.Value.UserId].OnlineMeetings[meetingId]
                    .AttendanceReports[currentAttendanceReport.Id]
                    .AttendanceRecords
                    .Request()
                    .GetAsync();

                    do
                    {
                        meetingAttendanceReport.ParticipantsInfo.AddRange(attendanceRecords.CurrentPage
                            .Select(attendanceRecord => new MeetingParticipantRecord
                            {
                                Id = attendanceRecord.Id,
                                DisplayName = attendanceRecord.Identity.DisplayName,
                                Duration = GetDurationFormattedString((int)attendanceRecord.TotalAttendanceInSeconds),
                                FirstJoinTime = "{{TIME(" + ((DateTimeOffset)attendanceRecord.AttendanceIntervals.FirstOrDefault().JoinDateTime).DateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}",
                                LastLeaveTime = "{{TIME(" + ((DateTimeOffset)attendanceRecord.AttendanceIntervals.LastOrDefault().LeaveDateTime).DateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}",
                                Role = attendanceRecord.Role
                            }));

                        // If there are more result.
                        if (attendanceRecords.NextPageRequest != null)
                        {
                            attendanceRecords = await attendanceRecords.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (attendanceRecords.CurrentPage != null);

                    return meetingAttendanceReport;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Converts the time in seconds to formatted string in {hour}h {minute}m {second}s format.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns></returns>
        private string GetDurationFormattedString(int timeInSeconds)
        {
            var durationString = string.Empty;

            if (timeInSeconds / 60 > 0)
            {
                var minute = 0;
                if (timeInSeconds / 60 >= 60)
                {
                    minute = timeInSeconds / 60;
                    durationString += (minute / 60).ToString() + "h ";
                }

                if (minute > 0)
                {
                    Console.WriteLine(minute);
                    durationString += (minute % 60).ToString() + "m ";
                }
                else
                {
                    durationString += (timeInSeconds / 60).ToString() + "m ";
                }
            }

            if (timeInSeconds % 60 > 0)
            {
                durationString += $"{(timeInSeconds % 60).ToString()}s ";
            }

            return durationString;
        }


        /// <summary>
        /// Gets the formatted meeting start and end string for Adaptive Card.
        /// </summary>
        /// <param name="startTime">Meeting start time.</param>
        /// <param name="endTime">Meeting end time.</param>
        /// <returns>Formatted time string.</returns>
        private string GetStartEndTimeFormattedStringForAdaptiveCard(DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            var startTimeString = "{{TIME(" + ((DateTimeOffset)startTime).DateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}";
            var endTimeString = "{{TIME(" + ((DateTimeOffset)endTime).DateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}";


            return startTimeString + " - " + endTimeString;
        }
    }
}
