// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using CallingBotSample.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;

namespace CallingBotSample.Services.CognitiveServices
{
    /// <inheritdoc/>
    public class SpeechService : ISpeechService
    {
        private readonly IWebHostEnvironment environment;
        private readonly CognitiveServicesOptions cognitiveServicesOptions;

        public SpeechService(IWebHostEnvironment environment, IOptions<CognitiveServicesOptions> cognitiveServicesOptions)
        {
            this.environment = environment;
            this.cognitiveServicesOptions = cognitiveServicesOptions.Value;
        }

        /// <inheritdoc/>
        public async Task<SpeechRecognitionResult> ConvertWavToText(string fileName)
        {
            if (!cognitiveServicesOptions.Enabled)
            {
                throw new Exception("Cognitive services is not enabled.");
            }

            var speechConfig = SpeechConfig.FromSubscription(cognitiveServicesOptions.SpeechKey, cognitiveServicesOptions.SpeechRegion);
            speechConfig.SpeechRecognitionLanguage = cognitiveServicesOptions.SpeechRecognitionLanguage;

            var fullFilePath = Path.Combine(environment.WebRootPath, fileName);

            using var audioConfig = AudioConfig.FromWavFileInput(fullFilePath);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            return await speechRecognizer.RecognizeOnceAsync();
        }
    }
}
