// <copyright file="ParticipantDetail.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace AppIconBadgingInMeetings.Models
{
    /// <summary>
    /// Represents the details of a participant in a meeting.
    /// </summary>
    public class ParticipantDetail
    {
        /// <summary>
        /// Gets or sets the recipient Id of the participant.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the participant.
        /// </summary>
        public string Name { get; set; }
    }

}
