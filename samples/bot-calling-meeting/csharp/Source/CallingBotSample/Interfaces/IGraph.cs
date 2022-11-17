// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Graph;

namespace CallingBotSample.Interfaces
{
    /// <summary>
    /// Interface for Graph.
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// Creates Online Event.
        /// </summary>
        /// <returns>online event.</returns>
        Task<OnlineMeeting> CreateOnlineMeetingAsync();

        /// <summary>
        /// Creates call.
        /// </summary>
        /// <returns>Call.</returns>
        Task<Call> CreateCallAsync();

        /// <summary>
        /// Transfer call.
        /// </summary>
        /// <returns>Call.</returns>
        Task TransferCallAsync(string replaceCallId);

        /// <summary>
        /// Join Scheduled Meeting.
        /// </summary>
        /// <returns>JoinScheduledMeeting.</returns>
        Task<Call> JoinScheduledMeeting(string meetingUrl);

        /// <summary>
        /// Invite Participant to Meeting.
        /// </summary>
        /// <returns>JoinScheduledMeeting.</returns>
        void InviteParticipant(string meetingId);

        /// <summary>
        /// Play a defined prompt in a meeting
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        Task PlayPrompt(string meetingId);
    }
}
