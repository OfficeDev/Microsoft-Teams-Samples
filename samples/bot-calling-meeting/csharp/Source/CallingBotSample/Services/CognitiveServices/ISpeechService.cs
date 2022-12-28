// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

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
        /// <returns>The result text of the recognition of the recording. Null if recognition failed</returns>
        Task<string?> ConvertWavToText(string fileName);

        /// <summary>
        /// Convert text to speech recording
        /// </summary>
        /// <param name="text">The text to covert to speech</param>
        /// <returns>The path to the audio file of the converted text. Null if conversion failed</returns>
        Task<string?> ConvertTextToSpeech(string text);
    }
}
