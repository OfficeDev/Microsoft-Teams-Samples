// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// Defines all the <see cref="Member"/> roles.
    /// </summary>
    public enum MemberRole
    {
        /// <summary>
        /// Educator - responsible for a course.
        /// </summary>
        Educator,

        /// <summary>
        /// Tutor - responsible for a group of students.
        /// </summary>
        Tutor,

        /// <summary>
        /// Student.
        /// </summary>
        Student,
    }
}
