// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;
using Microsoft.Graph;

public interface ICallService
{
    Task Answer(string id, params MediaInfo[]? preFetchMedia);

    Task<Call> Create(params Identity[] users);

    Task<Call> Get(string id);

    /// <summary>
    /// Delete/Hang up a call
    /// </summary>
    /// <returns></returns>
    Task HangUp(string id);

    Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts);

    Task<Call> Reject(string id);

    /// <summary>
    /// Redirect a call that has not been answered yet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Call> Redirect(string id);

    Task Transfer(string id, Identity transferIdentity, Identity? transfereeIdentity = null);
}
