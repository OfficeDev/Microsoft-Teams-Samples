// <copyright file="IGraph.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CallingBotSample.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Graph;

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

    }
}
