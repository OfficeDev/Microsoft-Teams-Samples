// <copyright file="Meeting.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Meeting entity definition.
    /// </summary>
    public class Meeting
    {
        /// <summary>
        /// Gets or sets external Id for a meeting.
        ///
        /// External Id is a unique for every meeting.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets meeting Subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets list of participants in the meeting.
        /// </summary>
        public IEnumerable<ParticipantInfo> Participants { get; set; }

        /// <summary>
        /// Gets or sets meeting start datetime.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets meeting end datetime.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets meeting graph reosurce id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets join meeting url.
        /// </summary>
        public string JoinUrl { get; set; }

        /// <summary>
        /// Gets or sets meeting chat id.
        /// </summary>
        public string ChatId { get; set; }
    }
}
