// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// Channel Model class.
    /// </summary>
    public sealed class Channel
    {
        /// <summary>
        /// Gets or sets channel's Team Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets channel's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets channel's course id.
        /// </summary>
        public string CourseId { get; set; }
    }
}
