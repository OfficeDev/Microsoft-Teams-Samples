// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Interfaces;
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
