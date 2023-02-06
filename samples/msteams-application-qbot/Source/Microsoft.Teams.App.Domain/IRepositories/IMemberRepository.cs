// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="Member"/> repository.
    /// </summary>
    public interface IMemberRepository
    {
        /// <summary>
        /// Adds a list of course members.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="members">List of members.</param>
        /// <returns>Async task.</returns>
        Task AddCourseMembersAsync(string courseId, IEnumerable<Member> members);

        /// <summary>
        /// Updates course members.
        /// </summary>
        /// <param name="courseId">Course Id.</param>
        /// <param name="member">List of membrs/</param>
        /// <returns>async task.</returns>
        Task UpdateCourseMemberAsync(string courseId, Member member);

        /// <summary>
        /// Removes course members.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="memberIds">List of member id.</param>
        /// <returns>async task.</returns>
        Task RemoveCourseMembersAsync(string courseId, IEnumerable<string> memberIds);

        /// <summary>
        /// Deletes tutorial group members.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <param name="memberIds">Members.</param>
        /// <returns>Async task.</returns>
        Task RemoveTutorialGroupMemberAsync(string tutorialGroupId, IEnumerable<string> memberIds);

        /// <summary>
        /// Gets all the members in a course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>List of members.</returns>
        Task<IEnumerable<Member>> GetCourseMembersAsync(string courseId);

        /// <summary>
        /// Get course members with specified roles.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="roles">List of roles.</param>
        /// <returns>List of members.</returns>
        Task<IEnumerable<Member>> GetCourseMembersAsync(string courseId, IList<MemberRole> roles);

        /// <summary>
        /// Gets course member.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <param name="memberId">Member id.</param>
        /// <returns>Member.</returns>
        Task<Member> GetCourseMemberAsync(string courseId, string memberId);

        /// <summary>
        /// Gets tutorial group membrs.
        /// </summary>
        /// <param name="tutorialGroupId">Tutorial group id.</param>
        /// <returns>List of members.</returns>
        Task<IEnumerable<Member>> GetTutorialGroupMembersAsync(string tutorialGroupId);
    }
}
