// <copyright file="KnowledgeBaseEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Knowledge base entity.
    /// </summary>
    internal class KnowledgeBaseEntity
    {
        /// <summary>
        /// Gets or sets KnowledgeBase's id.
        /// </summary>
        /// <remarks>This is an internal Id. This does not map to QnA Maker service knowledge base id.</remarks>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets KnowledgeBase's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's AAD Id who owns the knowledge base.
        /// </summary>
        public string OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets collection of courses.
        /// </summary>
        public virtual ICollection<CourseEntity> Courses { get; set; }
    }
}
