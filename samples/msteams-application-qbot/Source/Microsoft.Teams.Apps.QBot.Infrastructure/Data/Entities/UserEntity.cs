namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// User entity.
    /// </summary>
    internal class UserEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEntity"/> class.
        /// </summary>
        public UserEntity()
        {
            this.CourseMembership = new List<CourseMemberEntity>();
            this.TutorialGroupMembership = new List<TutorialGroupMemberEntity>();
            this.KnowledgeBases = new List<KnowledgeBaseEntity>();
        }

        /// <summary>
        /// Gets or sets user's Team Id.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets member's AAD Id.
        /// </summary>
        public string AadId { get; set; }

        /// <summary>
        /// Gets or sets member's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's principal name.
        /// </summary>
        /// <remarks>
        /// This will almost always be set to the user's organizational email address.
        /// </remarks>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets collection of courses.
        /// </summary>
        public virtual ICollection<CourseMemberEntity> CourseMembership { get; set; }

        /// <summary>
        /// Gets or sets collection of tutorial groups.
        /// </summary>
        public virtual ICollection<TutorialGroupMemberEntity> TutorialGroupMembership { get; set; }

        /// <summary>
        /// Gets or sets collection of knowledge bases.
        /// </summary>
        public virtual ICollection<KnowledgeBaseEntity> KnowledgeBases { get; set; }
    }
}
