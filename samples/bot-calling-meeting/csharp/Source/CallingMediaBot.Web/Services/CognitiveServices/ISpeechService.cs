// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.CognitiveServices;

using Microsoft.CognitiveServices.Speech;

public interface ISpeechService
{
    Task<SpeechRecognitionResult> ConvertWavToText(string fileName);
}
