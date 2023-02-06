// <copyright file="MeetingAttendanceSummary.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingAttendance.Models
{
    using System.Collections.Generic;

    public class MeetingAttendanceSummary
    {
        /// <summary>
        /// Id of attendace report.
        /// </summary>
        public string AttendaceReportId { get; set; }

        /// <summary>
        /// Total count of participants in a meeting.
        /// </summary>
        public int ParticipantCount { get; set; }

        /// <summary>
        /// Meeting start and end time formatted string.
        /// </summary>
        public string MeetingStartAndEndTime { get; set; }

        /// <summary>
        /// Meeting duration formatted string.
        /// </summary>
        public string MeetingDuration { get; set; }

        /// <summary>
        /// List of participants details.
        /// </summary>
        public List<MeetingParticipantRecord> ParticipantsInfo { get; set; }
    }
}
