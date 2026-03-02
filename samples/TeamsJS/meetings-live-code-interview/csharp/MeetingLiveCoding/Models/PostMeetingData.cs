// <copyright file="PostMeetingData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

namespace MeetingLiveCoding.Models
{
    /// <summary>
    /// Meeting detail mapping class.
    /// </summary>
    public class PostMeetingData
    {
        public string QuestionId { get; set; }
        public string MeetingId { get; set; }

        public string EditorData { get; set; }
    }
}