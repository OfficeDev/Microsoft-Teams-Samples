namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Course Entity.
    /// </summary>
    internal class CourseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CourseEntity"/> class.
        /// </summary>
        public CourseEntity()
        {
            this.Members = new HashSet<CourseMemberEntity>();
            this.Channels = new HashSet<ChannelEntity>();
            this.TutorialGroups = new HashSet<TutorialGroupEntity>();
        }

        /// <summary>
        /// Gets or sets the Id of the Course.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Bot Framework id of the Team associated with this Course.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory Id for the Team.
        /// </summary>
        public string TeamAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets Course name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets knowledge base id for the course.
        /// </summary>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets collection of channel entities.
        /// </summary>
        public virtual ICollection<ChannelEntity> Channels { get; set; }

        /// <summary>
        /// Gets or sets collection of course member entities.
        /// </summary>
        public virtual ICollection<CourseMemberEntity> Members { get; set; }

        /// <summary>
        /// Gets or sets collection of tutorial groups.
        /// </summary>
        public virtual ICollection<TutorialGroupEntity> TutorialGroups { get; set; }
    }
}
