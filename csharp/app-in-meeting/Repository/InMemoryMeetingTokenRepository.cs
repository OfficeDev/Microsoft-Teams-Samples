// <copyright file="InMemoryMeetingTokenRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Repository
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TokenApp.Models;

    /// <summary>
    /// In-memory implementation of <see cref="IMeetingTokenRepository"/>.
    /// </summary>
    public class InMemoryMeetingTokenRepository : IMeetingTokenRepository
    {
        private static readonly object SyncObject = new object();

        private readonly ConcurrentDictionary<string, MeetingSummary> meetingTokenStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMeetingTokenRepository"/> class.
        /// </summary>
        public InMemoryMeetingTokenRepository()
        {
            this.meetingTokenStore = new ConcurrentDictionary<string, MeetingSummary>();
        }

        /// <inheritdoc/>
        public Task<MeetingSummary> GetMeetingSummaryAsync(string meetingId)
        {
            MeetingSummary meetingToken = default;
            var currMeeting = this.meetingTokenStore.GetOrAdd(meetingId, CreateNewMeetingData);
            lock (SyncObject)
            {
                meetingToken = currMeeting.Clone();
            }

            return Task.FromResult(meetingToken);
        }

        /// <inheritdoc/>
        public Task<UserToken> GenerateTokenAsync(string meetingId, UserInfo userInfo)
        {
            var currMeeting = this.meetingTokenStore.GetOrAdd(meetingId, CreateNewMeetingData);

            UserToken newUserToken = default;
            var userToken = currMeeting.GetorAddToken(userInfo);
            lock (SyncObject)
            {
                newUserToken = userToken.Clone();
            }

            return Task.FromResult(newUserToken);
        }

        /// <inheritdoc/>
        public Task<MeetingSummary> AcknowledgeTokenAsync(string meetingId, string userId)
        {
            var currMeeting = this.meetingTokenStore.GetOrAdd(meetingId, CreateNewMeetingData);
            var updatedToken = currMeeting.UpdateTokenStatus(TokenStatus.Serviced, userId);

            MeetingSummary updatedMeetingToken = default;
            lock (SyncObject)
            {
                updatedMeetingToken = updatedToken.Clone();
            }

            return Task.FromResult(updatedMeetingToken);
        }

        /// <inheritdoc/>
        public Task<MeetingSummary> SkipTokenAsync(string meetingId)
        {
            var currMeeting = this.meetingTokenStore.GetOrAdd(meetingId, CreateNewMeetingData);
            var updatedToken = currMeeting.UpdateTokenStatus(TokenStatus.NotUsed);

            MeetingSummary updatedMeetingToken = default;
            lock (SyncObject)
            {
                updatedMeetingToken = updatedToken.Clone();
            }

            return Task.FromResult(updatedMeetingToken);
        }

        private static MeetingSummary CreateNewMeetingData(string meetingId)
        {
            return new MeetingSummary
            {
                MeetingMetadata = new MeetingMetadata
                {
                    MeetingId = meetingId,
                    CurrentToken = 0,
                    MaxTokenIssued = 0,
                },
                UserTokens = new List<UserToken>(),
            };
        }
    }
}