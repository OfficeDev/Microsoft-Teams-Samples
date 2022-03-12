// <copyright file="BotMessagesController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.UserSpecificViews.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;

    /// <summary>
    /// The BotMessagesController is reponsible for connecting the Asp.Net MVC pipeline to the <see cref="IBotFrameworkHttpAdapter" />
    /// and the underlying activity handler (<see cref="IBot" />).
    /// </summary>
    [Route("/api/messages")]
    [ApiController]
    public class BotMessagesController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter botFrameworkHttpAdapter;
        private readonly IBot activityHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotMessagesController"/> class.
        /// </summary>
        /// <param name="adapter">The BotFramework adapter.</param>
        /// <param name="activityHandler">The underlying activity handler.</param>
        public BotMessagesController(IBotFrameworkHttpAdapter adapter, IBot activityHandler)
        {
            botFrameworkHttpAdapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.activityHandler = activityHandler ?? throw new ArgumentNullException(nameof(activityHandler));
        }

        /// <summary>
        /// Handle inbound messages from the BotFramework service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for the underlying request.</param>
        /// <returns>A Task that resolves when the message is processed.</returns>
        [HttpPost]
        public async Task PostAsync(CancellationToken cancellationToken)
        {
            await botFrameworkHttpAdapter
                .ProcessAsync(
                    httpRequest: Request,
                    httpResponse: Response,
                    bot: activityHandler,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}