// <copyright file="TutorialGroupMemberEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    /// <summary>
    /// Tutorial group member entity.
    /// </summary>
    internal sealed class TutorialGroupMemberEntity
    {
        /// <summary>
        /// Gets or sets the Id of the Tutorial Group.
        /// </summary>
        public string TutorialGroupId { get; set; }

        /// <summary>
        /// Gets or sets tutorial group.
        /// </summary>
        public TutorialGroupEntity TutorialGroup { get; set; }

        /// <summary>
        /// Gets or sets course id.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets user's Team Id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets user.
        /// </summary>
        public UserEntity User { get; set; }
    }
}
