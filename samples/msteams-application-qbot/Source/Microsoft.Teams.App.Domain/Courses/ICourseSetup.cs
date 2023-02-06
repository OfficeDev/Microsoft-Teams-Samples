namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Holds operations involed in Setting-up a course.
    ///
    /// Responsible for the following:
    /// 1. Managing a course - Add, update and delete.
    /// 2. Managing channels in a course - Add, update and delete.
    /// 3. Managing members in a course - Add and remove.
    /// 4. Managing tutorial groups in a course - Add and delete.
    /// 5. Linking KB to a course.
    /// </summary>
    public interface ICourseSetup
    {
        /// <summary>
        /// Adds a new Course.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <param name="channels">List of channels in the course.</param>
        /// <param name="members">List of members in the course.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddNewCourseAsync(Course course, IEnumerable<Channel> channels, IEnumerable<Member> members);

        /// <summary>
        /// Updates an existing Course.
        ///
        /// Note: Only name changes allowed.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateCourseAsync(Course course);

        /// <summary>
        /// Deletes an existing Course and related entities.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteCourseAsync(string courseId);

        /// <summary>
        /// Adds channel to a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channel">Channel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddChannelAsync(string courseId, Channel channel);

        /// <summary>
        /// Updates channel in a course.
        ///
        /// Note: Only name updates are allowed.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channel">Channel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateChannelAsync(string courseId, Channel channel);

        /// <summary>
        /// Removes channel from a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="channelId">Channel Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteChannelAsync(string courseId, string channelId);

        /// <summary>
        /// Adds members to a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="members">List of members.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddMembersAsync(string courseId, IEnumerable<Member> members);

        /// <summary>
        /// Updates member in a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="member">Updated member object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateMemberAsync(string courseId, Member member);

        /// <summary>
        /// Removes members from a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="members">List of members.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveMembersAsync(string courseId, IEnumerable<Member> members);

        /// <summary>
        /// Adds tutorial groups to a course.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="tutorialGroups">List of tutorial groups.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddTutorialGroupsAsync(string courseId, IEnumerable<TutorialGroup> tutorialGroups);

        /// <summary>
        /// Deletes tutorial groups from a course.
        /// </summary>
        /// <param name="tutorialGroupIds">List of tutorial group Ids.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteTutorialGroupsAsync(IEnumerable<string> tutorialGroupIds);
    }
}
