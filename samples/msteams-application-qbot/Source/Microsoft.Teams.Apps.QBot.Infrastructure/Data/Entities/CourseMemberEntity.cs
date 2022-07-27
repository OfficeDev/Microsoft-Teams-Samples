// <copyright file="CourseMemberEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Course member entity.
    /// </summary>
    internal class CourseMemberEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CourseMemberEntity"/> class.
        /// </summary>
        public CourseMemberEntity()
        {
            this.TutorialGroupsMembership = new HashSet<TutorialGroupMemberEntity>();
        }

        /// <summary>
        /// Gets or sets the Id of the Course.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets course entity.
        /// </summary>
        public CourseEntity Course { get; set; }

        /// <summary>
        /// Gets or sets member's Team Id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets user entity.
        /// </summary>
        public UserEntity User { get; set; }

        /// <summary>
        /// Gets or sets User Role - Student, Educator and Tutor.
        /// </summary>
        public string MemberRole { get; set; }

        /// <summary>
        /// Gets or sets tutorial groups membership collection.
        /// </summary>
        public virtual ICollection<TutorialGroupMemberEntity> TutorialGroupsMembership { get; set; }
    }
}
