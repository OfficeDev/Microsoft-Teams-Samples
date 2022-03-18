// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities
{
    /// <summary>
    /// User Entity contains the attributes related to a user
    /// Also relationships with other entities.
    /// </summary>
    public class UserEntity
    {
        public UserEntity()
        {
            this.DocumentViewers = new List<DocumentViewerEntity>();
        }

        /// <summary>
        /// Gets or sets UserId / AAD ID
        /// </summary>

        public string UserId { get; set; }

        public virtual ICollection<DocumentViewerEntity> DocumentViewers { get; set; }
    }
}
