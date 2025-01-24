// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    /// <summary>
    /// Bot implementation for starting a new thread in a Microsoft Teams channel.
    /// </summary>
    public class TeamsStartNewThreadInTeam : ActivityHandler
    {
        private readonly string _appId;

        public TeamsStartNewThreadInTeam(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"];
        }

        /// <summary>
        /// Handles incoming messages and starts a new thread in the Teams channel, then sends a response in that thread.
        /// </summary>
        /// <param name="turnContext">The turn context containing the message activity.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Retrieve the channel ID for the current message context.
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();

            // Create a message activity to send to the Teams channel.
            var activity = MessageFactory.Text("Starting a new thread in the specified channel.");

            try
            {
                // Send a message to the Teams channel and retrieve details for continuing the conversation.
                var details = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, activity, teamsChannelId, _appId, cancellationToken);

                // Continue the conversation in the new thread.
                await ((CloudAdapter)turnContext.Adapter).ContinueConversationAsync(
                    botAppId: _appId,
                    reference: details.Item1,
                    callback: async (t, ct) =>
                    {
                        // Send the first response in the newly created thread.
                        await t.SendActivityAsync(MessageFactory.Text("This is the first response in the newly created thread"), ct);
                    },
                    cancellationToken: cancellationToken);

        }
    }
}
