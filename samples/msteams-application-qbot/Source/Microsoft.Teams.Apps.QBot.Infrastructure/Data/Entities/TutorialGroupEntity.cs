// <copyright file="TutorialGroupEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// TutorialGroup entity.
    /// </summary>
    internal class TutorialGroupEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TutorialGroupEntity"/> class.
        /// </summary>
        public TutorialGroupEntity()
        {
            this.Members = new HashSet<TutorialGroupMemberEntity>();
        }

        /// <summary>
        /// Gets or sets the Id of the Tutorial Group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the id the course.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets the human readable display name for the Tutorial Group.
        /// </summary>
        /// <remarks>
        /// This would be something like "Economics 101 - Morning Precept" that would be recognizable to the students
        /// and external users.
        /// </remarks>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user defined short code of the tutorial group.
        /// </summary>
        /// <remarks>
        /// This would be something like "ECON101 P01" which is recognizable to the course administrators.
        /// </remarks>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets collection of tutorial group members.
        /// </summary>
        public virtual ICollection<TutorialGroupMemberEntity> Members { get; set; }
    }
}
