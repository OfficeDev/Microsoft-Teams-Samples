// <copyright file="Transcripts.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscription.Models
{
    using System;

    public class Transcripts
    {
        /// <summary>
        /// Id of transcript.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Created date of trancript.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }
    }
}
