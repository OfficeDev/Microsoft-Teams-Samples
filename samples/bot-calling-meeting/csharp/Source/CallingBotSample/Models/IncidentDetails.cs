// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Graph;

namespace CallingBotSample.Models
{
    public class IncidentDetails
    {
        /// <summary>
        /// The Incident's call's ID
        /// </summary>
        public string? CallId { get; set; }

        /// <summary>
        /// Subject of the incident. This will be spoken in the meeting
        /// </summary>
        public string? IncidentSubject { get; set; }

        /// <summary>
        /// Meeting's info, used for creating the call
        /// </summary>
        public MeetingInfo? MeetingInfo { get; set; }

        /// <summary>
        /// Meeting's chat info, used for creating the call
        /// </summary>
        public ChatInfo? ChatInfo { get; set; }

        /// <summary>
        /// The start time of the meeting, used in the Adaptive Card with the details shown in the chat
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end time of the meeting, used in the Adaptive Card with the details shown in the chat
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Meeting participants, used to ensure the participants are invited to the call
        /// </summary>
        public IEnumerable<Identity> Participants { get; set; } = new List<Identity>();
    }
}
