// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites;

    /// <summary>
    /// Model for a Document, shared between EF and a requests
    /// </summary>
    public class DocumentEntity
    {
        /// <summary>
        /// DocumentEntity contains Document and the related entities with relationships.
        /// </summary>
        public DocumentEntity()
        {
            this.SignatureEntities = new List<SignatureEntity>();
            this.DocumentViewers = new HashSet<DocumentViewerEntity>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the DocumentType
        /// A document is classifed into one of the types like Purchase Agreement, Sales Contract
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Gets or sets OwnerId
        /// The fetched AzureAD Object Id of the user creating the document
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets DocumentState
        /// Contains the possible states for a document - active, staqged, completed
        /// </summary>
        public string DocumentState { get; set; }

        /// <summary>
        /// Gets or sets the Viewers of the document
        /// </summary>
        public ICollection<ViewerEntity> ViewerEntities { get; set; }


        /// <summary>
        /// Gets or sets the SignatureEntities belonging to a document
        /// </summary>
        /// Re-name
        public virtual IList<SignatureEntity> SignatureEntities { get; set; }

        /// <summary>
        /// Gets or sets DocumentViewers.
        /// The virtual collection establishes indirect many to many relationship
        /// between a document and an entity
        /// </summary>
        public virtual ICollection<DocumentViewerEntity> DocumentViewers { get; set; }

    }
}
