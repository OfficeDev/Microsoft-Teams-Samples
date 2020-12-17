// <copyright file="MeetingMetadata.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Models
{
    /// <summary>
    /// Meeting metadata model.
    /// </summary>
    public class MeetingMetadata
    {
        /// <summary>
        /// Gets or sets the meeting Id.
        /// </summary>
        public string MeetingId { get; set; }

        /// <summary>
        /// Gets or sets the current token.
        /// </summary>
        public int CurrentToken { get; set; }

        /// <summary>
        /// Gets or sets the max token issued up until the field access.
        /// </summary>
        public int MaxTokenIssued { get; set; }

        /// <summary>
        /// Gets the deep copy of meeting metadata.
        /// </summary>
        /// <returns>the meeting metadata instance.</returns>
        public MeetingMetadata Clone()
        {
            return new MeetingMetadata()
            {
                MeetingId = this.MeetingId,
                CurrentToken = this.CurrentToken,
                MaxTokenIssued = this.MaxTokenIssued,
            };
        }
    }
}