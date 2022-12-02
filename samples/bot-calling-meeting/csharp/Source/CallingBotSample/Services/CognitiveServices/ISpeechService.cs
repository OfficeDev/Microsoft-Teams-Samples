// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace CallingBotSample.Services.CognitiveServices
{
    /// <summary>
    /// Service for handling calls to the Cognitive Speech
    /// </summary>
    public interface ISpeechService
    {
        /// <summary>
        /// Convert the speech of a Wav recording to the text value of the recording
        /// </summary>
        /// <param name="fileName">The file name of the recording</param>
        /// <returns>The result of the recognition of the recording</returns>
        Task<SpeechRecognitionResult> ConvertWavToText(string fileName);
    }
}
