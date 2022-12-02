// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingBotSample.Options
{
    /// <summary>
    /// The Cognitive Services options class.
    /// </summary>
    public class CognitiveServicesOptions
    {
        /// <summary>
        /// Is the service enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Cognitive Services speech key
        /// </summary>
        public string? SpeechKey { get; set; }

        /// <summary>
        /// Cognitive Services speech region
        /// </summary>
        public string? SpeechRegion { get; set; }

        /// <summary>
        /// The language to use when recognising speech
        /// </summary>
        public string? SpeechRecognitionLanguage { get; set; }
    }
}
