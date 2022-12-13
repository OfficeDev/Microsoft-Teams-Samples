// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Graph;

namespace CallingBotSample.Models
{
    public class IncidentDetails
    {
        public string? CallId { get; set; }
        public string? IncidentSubject { get; set; }
        public MeetingInfo? MeetingInfo { get; set; }
        public ChatInfo? ChatInfo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IEnumerable<Identity>? Participants { get; set; }
    }
}
