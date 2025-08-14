// <copyright file="ParticipantDetail.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace InMeetingNotificationsBot.Models
{
    public class ParticipantDetail
    {
        /// <summary>
        ///  Recipient Id of the participant.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  Name of the participant.
        /// </summary>
        public string Name { get; set; }
    }

}
