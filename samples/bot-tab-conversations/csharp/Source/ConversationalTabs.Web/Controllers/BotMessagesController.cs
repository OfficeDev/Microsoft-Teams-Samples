// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

[ApiController]
[Route("api/messages")]
public class BotMessagesController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter _botFrameworkHttpAdapter;
    private readonly IBot _activityHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotMessagesController"/> class.
    /// </summary>
    /// <param name="adapter">The BotFramework adapter.</param>
    /// <param name="activityHandler">The underlying activity handler.</param>
    public BotMessagesController(IBotFrameworkHttpAdapter adapter, IBot activityHandler)
    {
        _botFrameworkHttpAdapter = adapter;
        _activityHandler = activityHandler;
    }


    /// <summary>
    /// Handle inbound messages from the BotFramework service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the underlying request.</param>
    /// <returns>A Task that resolves when the message is processed.</returns>
    [HttpPost]
    public async Task PostAsync(CancellationToken cancellationToken)
    {
        await _botFrameworkHttpAdapter
            .ProcessAsync(
                httpRequest: Request,
                httpResponse: Response,
                bot: _activityHandler,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
