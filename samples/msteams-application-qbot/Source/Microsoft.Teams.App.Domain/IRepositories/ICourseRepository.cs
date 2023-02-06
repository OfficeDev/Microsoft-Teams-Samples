namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="Course"/> repository.
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Adds a course to the db.
        /// </summary>
        /// <param name="course">Course object.</param>
        /// <returns>Async task.</returns>
        Task AddCourseAsync(Course course);

        /// <summary>
        /// Updates course object.
        /// </summary>
        /// <param name="course">Course object.</param>
        /// <returns>Async task.</returns>
        Task UpdateCourseAsync(Course course);

        /// <summary>
        /// Deletes course object.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>Async task.</returns>
        Task DeleteCourseAsync(string courseId);

        /// <summary>
        /// Gets course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>Course object.</returns>
        Task<Course> GetCourseAsync(string courseId);

        /// <summary>
        /// Gets all the courses.
        /// </summary>
        /// <returns>List of courses.</returns>
        Task<IEnumerable<Course>> GetAllCoursesAsync();

        /// <summary>
        /// Gets all the courses user is member of.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>List of courses.</returns>
        Task<IEnumerable<Course>> GetAllCoursesForUserAsync(string userId);
    }
}
