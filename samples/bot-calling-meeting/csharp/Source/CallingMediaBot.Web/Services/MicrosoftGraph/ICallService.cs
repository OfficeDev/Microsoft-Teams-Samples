// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;
using Microsoft.Graph;

public interface ICallService
{
    /// <summary>
    /// Answer a call
    /// </summary>
    /// <param name="id">The ID of the call to answer</param>
    /// <param name="preFetchMedia">Media that Teams will prefetch.</param>
    /// <returns>Task</returns>
    Task Answer(string id, params MediaInfo[]? preFetchMedia);

    /// <summary>
    /// Create a new call
    /// </summary>
    /// <param name="users">Users to add to the call</param>
    /// <returns>The calls details</returns>
    Task<Call> Create(params Identity[] users);

    /// <summary>
    /// Get a calls details
    /// </summary>
    /// <param name="id">The ID of the call</param>
    /// <returns>The calls details</returns>
    Task<Call> Get(string id);

    /// <summary>
    /// Delete/Hang up a call
    /// </summary>
    /// <returns>Task</returns>
    Task HangUp(string id);

    /// <summary>
    /// Plays a media prompt in a call
    /// </summary>
    /// <param name="id">The ID of the call where you want to play the prompt</param>
    /// <param name="mediaPrompts">The Media to play</param>
    /// <returns>The Play Prompt Operation</returns>
    Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts);

    /// <summary>
    /// Play the provided prompt in a call, and then record what is said.
    /// </summary>
    /// <param name="id">The ID of the call where you want to record</param>
    /// <param name="mediaPrompt">The media to play before recording</param>
    /// <param name="maxRecordDurationInSeconds">The maximum duration to record the response before stopping the recording</param>
    /// <param name="stopTones">Stop tones to stop the recording</param>
    /// <returns>The record operation with access token to and file location of the recording</returns>
    Task<RecordOperation> Record(
        string id,
        MediaInfo mediaPrompt,
        int maxRecordDurationInSeconds = 10,
        IEnumerable<string>? stopTones = null);

    /// <summary>
    /// Reject a call
    /// </summary>
    /// <param name="id">The ID of the call to reject</param>
    /// <returns></returns>
    Task<Call> Reject(string id);

    /// <summary>
    /// Redirect a call that has not been answered yet
    /// </summary>
    /// <param name="id">The ID of the call to redirect</param>
    /// <returns></returns>
    Task<Call> Redirect(string id);

    /// <summary>
    /// Transfer an ongoing call to another user
    /// </summary>
    /// <param name="id">The ID of the call to transfer</param>
    /// <param name="transferIdentity"></param>
    /// <param name="transfereeIdentity"></param>
    /// <returns></returns>
    Task Transfer(string id, Identity transferIdentity, Identity? transfereeIdentity = null);
}
