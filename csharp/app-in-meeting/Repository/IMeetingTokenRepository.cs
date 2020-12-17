// <copyright file="IMeetingTokenRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Repository
{
    using System.Threading.Tasks;
    using TokenApp.Models;

    /// <summary>
    /// Meeting Token repository.
    /// </summary>
    public interface IMeetingTokenRepository
    {
        /// <summary>
        /// Get the meeting summary by id.
        /// </summary>
        /// <param name="meetingId">the meeting Id.</param>
        /// <returns>the meeting summary.</returns>
        Task<MeetingSummary> GetMeetingSummaryAsync(string meetingId);

        /// <summary>
        /// Get or generate the user token for a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting Id.</param>
        /// <param name="userInfo">The user information.</param>
        /// <returns>the user token.</returns>
        Task<UserToken> GenerateTokenAsync(string meetingId, UserInfo userInfo);

        /// <summary>
        /// Process the user token for a meeting by Id as below :
        /// 1. Get the user token by userId from the token list.
        /// 2. Set the user token status as Serviced.
        /// 3. Set the next 'waiting' user in sequence as current.
        /// 4. Return the meeting summary.
        /// </summary>
        /// <param name="meetingId">The meeting Id.</param>
        /// <param name="userId">The user Id.</param>
        /// <returns>the meeting summary.</returns>
        Task<MeetingSummary> AcknowledgeTokenAsync(string meetingId, string userId);

        /// <summary>
        /// Process the user token for a meeting by Id as below :
        /// 1. Get the user token for current token number.
        /// 2. Set the user token status as Not Used.
        /// 3. Set the next 'waiting' user in sequence as current.
        /// 4. Return the meeting summary.
        /// </summary>
        /// <param name="meetingId">the meeting Id.</param>
        /// <returns>the meeting summary.</returns>
        Task<MeetingSummary> SkipTokenAsync(string meetingId);
    }
}