// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingExtension_SP.Models.Sharepoint
{
    /// <summary>
    /// Doclibrary for sharepoint
    /// </summary>
    public class DocumentLibrary : AssetCard
    {       
        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name { get; set; }

        public string LinkingUri { get; set; }
    }
}
