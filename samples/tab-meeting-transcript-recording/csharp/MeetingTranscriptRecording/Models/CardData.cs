// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;

namespace MeetingTranscriptRecording.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CardData
    {
        public string subject { get; set; }

        public string start { get; set; }

        public string end { get; set; }

        public string onlineMeetingId { get; set; }

        public string transcriptsId { get; set; }

        public string recordingId { get; set; }

        public string organizer { get; set; }

        public bool condition { get; set; }
    }

    public class TranscriptsRequestBody
    {
        public string meetingId { get; set; }

        public string transcriptsId { get; set; }
    }

    public class RecordingRequestBody
    {
        public string meetingId { get; set; }

        public string recordingId { get; set; }
    }

}
