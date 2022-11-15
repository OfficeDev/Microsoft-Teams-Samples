// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;

using Microsoft.Graph;

public interface IOnlineMeetingService
{
    /// <summary>
    /// Create a meeting. Tied to the
    /// </summary>
    /// <param name="subject">Subject of the meeting to create</param>
    /// <returns></returns>
    Task<OnlineMeeting> Create(string subject);

    /// <summary>
    ///
    /// </summary>
    /// <param name="meetingId"></param>
    /// <returns></returns>
    Task<OnlineMeeting> Get(string meetingId);
}
