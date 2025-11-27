// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsFileUpload.Models
{
    /// <summary>
    /// Contains information needed to download a file from Teams.
    /// </summary>
    public class FileDownloadInfo
    {
        /// <summary>
        /// Gets or sets the URL to download the file from.
        /// </summary>
        public string DownloadUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier for the file.
        /// </summary>
        public string UniqueId { get; set; }
        
        /// <summary>
        /// Gets or sets the file type/extension.
        /// </summary>
        public string FileType { get; set; }
    }
}
