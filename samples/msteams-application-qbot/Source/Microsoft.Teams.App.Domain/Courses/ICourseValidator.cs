namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Course object validator.
    /// </summary>
    internal interface ICourseValidator
    {
        /// <summary>
        /// Validates if course properties are valid.
        /// </summary>
        /// <param name="course">Course object.</param>
        /// <returns>If the course properties are valid.</returns>
        bool IsValid(Course course);

        /// <summary>
        /// Validates if channel properties are valid.
        /// </summary>
        /// <param name="channel">Channel object.</param>
        /// <returns>If the channel properties are valid.</returns>
        bool IsValid(Channel channel);
    }
}
