// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CallingBotSample.AdaptiveCards;
using CallingBotSample.Options;
using CallingBotSample.Services.MicrosoftGraph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CallingBotSample.Bots
{
    public class MessageBot : ActivityHandler
    {
        private readonly IAdaptiveCardFactory adaptiveCardFactory;
        private readonly ICallService callService;
        private readonly IEnumerable<UserOptions> users;

        public MessageBot(IAdaptiveCardFactory adaptiveCardFactory, ICallService callService, IOptions<List<UserOptions>> users)
        {
            this.adaptiveCardFactory = adaptiveCardFactory;
            this.callService = callService;
            this.users = users.Value;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                dynamic value = turnContext.Activity.Value;
                if (value != null)
                {
                    string type = value["type"];
                    type = string.IsNullOrEmpty(type) ? "." : type.ToLower();
                    await SendResponse(turnContext, type, cancellationToken);
                }
            }
            else
            {
                turnContext.Activity.RemoveRecipientMention();
                await SendResponse(turnContext, turnContext.Activity.Text.Trim().ToLower(), cancellationToken);
            }
        }

        private async Task SendResponse(ITurnContext<IMessageActivity> turnContext, string input, CancellationToken cancellationToken)
        {
            switch (input)
            {
                case "createcall":
                    var call = await callService.Create(users: new Identity
                    {
                        DisplayName = users.FirstOrDefault()?.DisplayName,
                        Id = users.FirstOrDefault()?.Id
                    });

                    if (call != null)
                    {
                        await turnContext.SendActivityAsync("Placed a call Successfully.", cancellationToken: cancellationToken);
                    }
                    break;
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardFactory.CreateWelcomeCard()), cancellationToken);
                    break;
            }
        }
    }
}
