// <copyright file="MeetingNotification.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;

namespace InMeetingNotificationsBot.Models
{
    public class MeetingNotification
    {   
        /// <summary>
        /// List of participants that are currently part of the meeting.
        /// </summary>
        public List<ParticipantDetail> ParticipantDetails { get; set; }
    }
}