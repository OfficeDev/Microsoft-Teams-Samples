// <copyright file="EventsData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using MeetingTranscriptRecording.Models;
using Microsoft.Graph;
using System.Xml.Linq;

namespace MeetingTranscriptRecording.Models
{
    /// <summary>
    /// Class for graph api response data
    /// </summary>
    public class ResponseEventData
    {
        public List<EventData> Value { get; set; }
    }

    public class JoinUrlData
    {

        public List<JoinWebUrl> Value { get; set; }
    }

    public class transcriptsID
    {

        public List<transcriptsIDs> Value { get; set; }
    }

    public class transcriptsIDs
    {
        public string id { get; set; }
    }

    public class JoinWebUrl
    {
        public string id { get; set; }
    }

    public class EventData
    {
        public bool isOnlineMeeting { get; set; }

        public string subject { get; set; }

        public Start start { get; set; }
        public End end { get; set; }

        public OnlineMeeting onlineMeeting { get; set; }

    }

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class OnlineMeeting
    {
        public string joinUrl { get; set; }
    }
}