namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using System;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Validates course.
    /// </summary>
    internal sealed class CourseValidator : ICourseValidator
    {
        /// <inheritdoc/>
        public bool IsValid(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            if (string.IsNullOrWhiteSpace(course.TeamId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(course.TeamAadObjectId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(course.Name))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool IsValid(Channel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (string.IsNullOrWhiteSpace(channel.Id))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(channel.Name))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(channel.CourseId))
            {
                return false;
            }

            return true;
        }
    }
}
