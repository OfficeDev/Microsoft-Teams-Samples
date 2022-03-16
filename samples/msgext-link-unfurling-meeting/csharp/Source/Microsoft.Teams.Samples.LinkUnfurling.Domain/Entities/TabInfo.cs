// <copyright file="TabInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Tab info entity.
    /// </summary>
    public class TabInfo
    {
        /// <summary>
        /// Gets or sets tab display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets tab entity id.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets tab content url.
        /// </summary>
        public string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets fallback website url.
        /// </summary>
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets remove tab url.
        /// </summary>
        public string RemoveUrl { get; set; }
    }
}
