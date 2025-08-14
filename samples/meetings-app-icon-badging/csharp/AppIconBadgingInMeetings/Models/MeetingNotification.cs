// <copyright file="MeetingNotification.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;

namespace AppIconBadgingInMeetings.Models
{
    /// <summary>
    /// Represents a notification for a meeting, including a list of participants.
    /// </summary>
    public class MeetingNotification
    {
        /// <summary>
        /// Gets or sets the list of participants that are currently part of the meeting.
        /// </summary>
        public List<ParticipantDetail> ParticipantDetails { get; set; }
    }
}