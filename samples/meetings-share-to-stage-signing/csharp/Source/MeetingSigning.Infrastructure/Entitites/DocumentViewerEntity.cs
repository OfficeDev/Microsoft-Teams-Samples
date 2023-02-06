// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities
{
    /// <summary>
    /// DocumentViewerEntity class contains a many to many relationship
    /// between Document and User entities
    /// </summary>
    public class DocumentViewerEntity
    {
        // Gets or Sets DocumentId
        // Primary key of Document Entity
        public Guid DocumentId { get; set; }

        // Gets or Sets Document
        // DocumentEntity navigation property
        public DocumentEntity Document { get; set; }

        // Gets or Sets UserId
        // Primary key of User Entity
        public string UserId { get; set; }

        // Gets or Sets Viewer
        // navigation property of User Entity
        public UserEntity Viewer { get; set; }
    }
}
