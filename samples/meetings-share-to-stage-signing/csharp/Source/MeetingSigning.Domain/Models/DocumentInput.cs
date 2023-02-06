// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// DocumentInput contains the input structure for web api
    /// </summary>
    public class DocumentInput
    {
        /// <summary>
        /// Gets or sets DocumentType of the document
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Gets or sets Viewers of the document.
        /// </summary>
        public IList<User> Viewers { get; set; }

        /// <summary>
        /// Gets or sets Signers of the document
        /// </summary>
        public IList<User> Signers { get; set; }
    }
}
