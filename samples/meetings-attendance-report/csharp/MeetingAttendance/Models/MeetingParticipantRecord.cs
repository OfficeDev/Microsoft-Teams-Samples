// <copyright file="MeetingParticipantRecord.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingAttendance.Models
{
    public class MeetingParticipantRecord
    {
        /// <summary>
        /// AAD object Id of participant.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of participant.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Formatted string of first join time of participant.
        /// </summary>
        public string FirstJoinTime { get; set; }

        /// <summary>
        /// Formatted string of last leave time of participant.
        /// </summary>
        public string LastLeaveTime { get; set; }

        /// <summary>
        /// Meeting duration of participant.
        /// </summary>
        public string Duration { get; set; }
        
        /// <summary>
        /// Role of participant.
        /// </summary>
        public string Role { get; set; }
    }
}
