// <copyright file="IGraph.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CallingMediaBot.Web.Interfaces
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
        /// Join Scheduled Meeting.
        /// </summary>
        /// <returns>JoinScheduledMeeting.</returns>
        Task<Call> JoinScheduledMeeting(string meetingUrl);

        /// <summary>
        /// Invite Participant to Meeting.
        /// </summary>
        /// <returns>JoinScheduledMeeting.</returns>
        Task InviteParticipant(string meetingId);
    }
}
