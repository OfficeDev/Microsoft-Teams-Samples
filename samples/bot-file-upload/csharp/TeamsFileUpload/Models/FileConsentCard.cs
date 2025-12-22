// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsFileUpload.Models
{
    /// <summary>
    /// Represents a file consent card for requesting user permission to upload a file.
    /// </summary>
    public class FileConsentCard
    {
        /// <summary>
        /// The content type identifier for file consent cards.
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.teams.card.file.consent";
        
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the file.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the context data to include when user accepts.
        /// </summary>
        public object AcceptContext { get; set; }
        
        /// <summary>
        /// Gets or sets the context data to include when user declines.
        /// </summary>
        public object DeclineContext { get; set; }
    }
}
