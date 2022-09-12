// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// The Course model class.
    /// </summary>
    public sealed class Course
    {
        /// <summary>
        /// Gets or sets the Id of the Course
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Bot Framework id of the Team associated with this Course.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory Id for the Team
        /// </summary>
        public string TeamAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets Course name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the course's picture url. Base64 encoded image.
        /// </summary>
        public string ProfilePicUri { get; set; }

        /// <summary>
        /// Gets or sets knowledge base id for the course.
        /// </summary>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the course has tutorial groups configured or not.
        /// </summary>
        public bool HasTutorialGroups { get; set; }
    }
}