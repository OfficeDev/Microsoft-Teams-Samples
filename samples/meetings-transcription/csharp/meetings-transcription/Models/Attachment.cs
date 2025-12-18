// <copyright file="Attachment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace meetings_transcription.Models
{
    /// <summary>
    /// Represents an attachment for Teams messages.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Gets or sets the content type of the attachment.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content of the attachment.
        /// </summary>
        public object Content { get; set; }
    }
}
