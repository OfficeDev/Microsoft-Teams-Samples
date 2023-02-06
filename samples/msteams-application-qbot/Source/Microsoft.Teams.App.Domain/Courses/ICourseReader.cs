namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Course reader interface.
    /// </summary>
    public interface ICourseReader
    {
        /// <summary>
        /// Gets course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>Course.</returns>
        Task<Course> GetCourseAsync(string courseId);

        /// <summary>
        /// Gets all the courses in the tenant.
        /// </summary>
        /// <returns>list of courses.</returns>
        Task<IEnumerable<Course>> GetAllCoursesAsync();

        /// <summary>
        /// Get all the courses that the user is member of.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>List of courses.</returns>
        Task<IEnumerable<Course>> GetAllCoursesForUserAsync(string userId);

        /// <summary>
        /// Gets all the channels in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>List of channels.</returns>
        Task<IEnumerable<Channel>> GetAllChannelsAsync(string courseId);

        /// <summary>
        /// Get channel in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Channel.</returns>
        Task<Channel> GetChannelAsync(string courseId, string channelId);

        /// <summary>
        /// Gets the member in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="memberId">Member id.</param>
        /// <returns>Member object.</returns>
        Task<Member> GetMemberAsync(string courseId, string memberId);

        /// <summary>
        /// Gets all the members in a course with roles specified.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="roles">List of member roles.</param>
        /// <returns>List of members.</returns>
        Task<IEnumerable<Member>> GetAllMembersAsync(string courseId, IList<MemberRole> roles = null);

        /// <summary>
        /// Gets all the tutorial groups in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>List of tutorial group.</returns>
        Task<IEnumerable<TutorialGroup>> GetAllTutorialGroupsAsync(string courseId);
    }
}
