namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Channel Entity.
    /// </summary>
    internal class ChannelEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEntity"/> class.
        /// </summary>
        public ChannelEntity()
        {
            this.Questions = new HashSet<QuestionEntity>();
        }

        /// <summary>
        /// Gets or sets channel's Team Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets channel's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets channel's course id.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets collection of question entities.
        /// </summary>
        public virtual ICollection<QuestionEntity> Questions { get; set; }
    }
}
