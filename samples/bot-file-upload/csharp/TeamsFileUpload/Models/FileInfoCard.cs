// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsFileUpload.Models
{
    /// <summary>
    /// Represents a file info card containing metadata about an uploaded file.
    /// </summary>
    public class FileInfoCard
    {
        /// <summary>
        /// The content type identifier for file info cards.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.teams.card.file.info";
        
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
