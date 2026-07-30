// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Model
{
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class MeetingResourceUpdate : MeetingResource
    {
        /// <summary>
        /// The list of users that joined.
        /// </summary>
        [JsonProperty(PropertyName = "activeParticipants@delta")]
        public List<MeetingParticipantInfo> ActiveParticipantJoined { get; set; }

        /// <summary>
        /// The list of users that left.
        /// </summary>
        [JsonProperty(PropertyName = "activeParticipants@remove")]
        public List<MeetingParticipantInfo> ActiveParticipantLeft { get; set; }
    }
}
