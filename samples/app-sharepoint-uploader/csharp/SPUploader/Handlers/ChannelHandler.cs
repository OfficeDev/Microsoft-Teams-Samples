// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Helpers;
using MeetingExtension_SP.Models;
using MessageExtension_SP.Helpers;
using MessageExtension_SP.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Constants = MessageExtension_SP.Helpers.Constants;

namespace MessageExtension_SP.Handlers
{
    /// <summary>
    /// This channel handler handles to post card in channel
    /// </summary>
    public class ChannelHandler
    {
        public async Task SendConversation(IConfiguration configuration)
        {
            Microsoft.Bot.Schema.Attachment card = CardHelper.CreateAdaptiveCardAttachment(Constants.UserCard, configuration);
            await Common.SendChannelData(card, configuration);

        }
    }
}