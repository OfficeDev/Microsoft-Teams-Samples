// <copyright file="IMeetingService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    using System.Threading.Tasks;

    /// <summary>
    /// Meetings service.
    /// </summary>
    public interface IMeetingService
    {
        /// <summary>
        /// Get the user's role in a meeting.
        /// This uses Meeting Extensibility Participant api.
        /// </summary>
        /// <param name="meetingId">The meeting id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <returns>A <see cref="Task{TResult}"/> returns the users's meeting role response.</returns>
        public Task<UserMeetingRoleServiceResponse> GetMeetingRoleAsync(string meetingId, string userId, string tenantId);

        /// <summary>
        /// Post a notification message and content bubble url to the chat represented by the conversation id.
        /// This uses Meeting Extensibility notification signal api.
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="tenantId">The tenant for which the notification has to be sent.</param>
        /// <param name="currentToken">The current token being serviced.</param>
        /// <param name="currentUserName">The name of the participant having the current token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returns the activity ID of the posted message.</returns>
        public Task PostStatusChangeNotification(string conversationId, string tenantId, int currentToken, string currentUserName);
    }
}