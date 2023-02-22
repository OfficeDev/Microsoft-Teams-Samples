// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// Model for a Document
    /// </summary>
    public class Document
    {
        public Document()
        {
            this.Signatures = new List<Signature>();
            this.Viewers = new List<Viewer>();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the DocumentType
        /// A document is classified into one of the types like Purchase Agreement, Sales Contract
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Gets or sets OwnerId
        /// The fetched AzureAD Object Id of the user creating the document.
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets DocumentState
        /// Contains the possible states for a document - active, completed.
        /// </summary>
        public DocumentState DocumentState => Signatures != default && !Signatures.All(s => s.IsSigned) ? DocumentState.Active : DocumentState.Complete;

        /// <summary>
        /// Gets or sets the Viewers of the document
        /// </summary>
        public ICollection<Viewer> Viewers { get; set; }

        /// <summary>
        /// Gets or sets the Signatures belonging to a document
        /// </summary>
        /// Re-name
        public ICollection<Signature> Signatures { get; set; }

        public Document DeepCopy()
        {
            Document other = (Document)this.MemberwiseClone();
            other.Signatures = this.Signatures.Select(signature => signature.DeepCopy()).ToList();
            other.Viewers = this.Viewers.Select(viewer => viewer.DeepCopy()).ToList();
            return other;
        }
    }
}
