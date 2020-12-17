// <copyright file="MeetingServiceOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    /// <summary>
    /// Options related to the <see cref="MeetingService"/>.
    /// </summary>
    public class MeetingServiceOptions
    {
        /// <summary>
        /// Gets or sets the MicrosoftAppId for the meeeting service.
        /// </summary>
        public string TeamsAppId { get; set; }

        /// <summary>
        /// Gets or sets the Content bubble iframe url.
        /// </summary>
        public string ContentBubbleUrl { get; set; }
    }
}