// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingBotSample.Options
{
    /// <summary>
    /// The Cognitive Services options class.
    /// </summary>
    public class CognitiveServicesOptions
    {
        public bool Enabled { get; set; }
        public string? SpeechKey { get; set; }
        public string? SpeechRegion { get; set; }
        public string? SpeechRecognitionLanguage { get; set; }
    }
}
