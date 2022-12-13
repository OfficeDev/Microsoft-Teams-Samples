// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using CallingBotSample.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CallingBotSample.Services.CognitiveServices
{
    /// <inheritdoc/>
    public class SpeechService : ISpeechService
    {
        private readonly IWebHostEnvironment environment;
        private readonly CognitiveServicesOptions cognitiveServicesOptions;
        private readonly ILogger<SpeechService> logger;

        public SpeechService(IWebHostEnvironment environment, IOptions<CognitiveServicesOptions> cognitiveServicesOptions, ILogger<SpeechService> logger)
        {
            this.environment = environment;
            this.cognitiveServicesOptions = cognitiveServicesOptions.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string?> ConvertWavToText(string fileName)
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

            var result = await speechRecognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return result.Text;
            }

            logger.LogTrace($"Speech was not recognized, result reason: {result.Reason}");
            return null;
        }

        public async Task<string?> ConvertTextToSpeech(string text)
        {
            if (!cognitiveServicesOptions.Enabled)
            {
                throw new Exception();
            }

            var speechConfig = SpeechConfig.FromSubscription(cognitiveServicesOptions.SpeechKey, cognitiveServicesOptions.SpeechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                SpeechSynthesisResult result = await speechSynthesizer.SpeakTextAsync(text);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    AudioDataStream stream = AudioDataStream.FromResult(result);

                    var outputFile = $"temp/{Guid.NewGuid()}.wav";
                    await stream.SaveToWaveFileAsync(Path.Combine(environment.WebRootPath, outputFile));

                    return outputFile;
                }
            }

            return null;
        }
    }
}
