// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// The Tutorial Group model class.
    /// </summary>
    public sealed class TutorialGroup
    {
        /// <summary>
        /// Gets or sets the Id of the Tutorial Group
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the course.
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
        /// Gets or sets the user defined id of short code of the tutorial group.
        /// </summary>
        /// <remarks>
        /// This would be something like "ECON101 P01" which is recognizable to the course administrators.
        /// </remarks>
        public string ShortCode { get; set; }
    }
}