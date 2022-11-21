// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace CallingBotSample.Services.CognitiveServices
{
    public interface ISpeechService
    {
        Task<string?> ConvertWavToText(string fileName);
        Task<string?> ConvertTextToSpeech(string text);
    }
}
