// <copyright file="ParticipantInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Meeting participant info entity.
    /// </summary>
    public class ParticipantInfo
    {
        /// <summary>
        /// Gets or sets user's AAD Id.
        /// </summary>
        public string AadId { get; set; }

        /// <summary>
        /// Gets or sets upn. User principal name of the participant.
        /// </summary>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets participant role in a meeting.
        /// </summary>
        public ParticipantRole Role { get; set; }
    }
}
