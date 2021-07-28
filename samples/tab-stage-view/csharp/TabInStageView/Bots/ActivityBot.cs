// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using TabInStageView.Helpers;

namespace TabInStageView.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(CardHelper.GetAdaptiveCardForTaskModule()));
        }
    }
}