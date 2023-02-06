// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CallingBotSample.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public class AudioRecordingConstants
    {
        public AudioRecordingConstants(IOptions<BotOptions> botOptions)
        {
            Speech = new MediaInfo
            {
                Uri = new Uri(botOptions.Value.BotBaseUrl, "audio/speech.wav").ToString(),
                ResourceId = Guid.NewGuid().ToString(),
            };

            PleaseRecordYourMessage = new MediaInfo
            {
                Uri = new Uri(botOptions.Value.BotBaseUrl, "audio/please-record-your-message.wav").ToString(),
                ResourceId = Guid.NewGuid().ToString(),
            };
        }

        public readonly MediaInfo Speech;
        public readonly MediaInfo PleaseRecordYourMessage;
    }
}
