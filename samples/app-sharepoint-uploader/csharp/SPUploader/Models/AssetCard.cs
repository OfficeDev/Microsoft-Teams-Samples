// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingExtension_SP.Models
{
    /// <summary>
    /// Asset card details
    /// </summary>
    public class AssetCard
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ServerRelativeUrl { get; set; }
        public string ImageUrl { get; set; }
        public string TimeCreated { get; set; }
        public string TimeLastModified { get; set; }
    }
}
