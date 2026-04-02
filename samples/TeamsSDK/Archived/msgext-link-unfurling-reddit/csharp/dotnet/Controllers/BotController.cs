// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

/// <summary>
/// The BotController is responsible for connecting the ASP.NET Core pipeline to the <see cref="IBotFrameworkHttpAdapter" />
/// and the underlying activity handler (<see cref="IBot" />).
/// </summary>
[Route("api/messages")]
[ApiController]
public class BotController(IBotFrameworkHttpAdapter adapter, IBot bot) : ControllerBase
{
    /// <summary>
    /// Handle inbound messages from the BotFramework service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the underlying request.</param>
    /// <returns>A Task that resolves when the message is processed.</returns>
    [HttpPost]
    public async Task PostAsync(CancellationToken cancellationToken)
    {
        await adapter.ProcessAsync(Request, Response, bot, cancellationToken);
    }
}