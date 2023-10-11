// <copyright file="EventsData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using MeetingTranscriptRecording.Models;
using Microsoft.Graph;
using System.Xml.Linq;

namespace MeetingTranscriptRecording.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ResponseEventData
    {
        public List<EventData> Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class JoinUrlData
    {
        public List<JoinWebUrl> Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class transcriptsData
    {
        public List<transcriptsId> Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class transcriptsId
    {
        public string id { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecordingData
    {
        public List<RecordingId> Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecordingId
    {
        public string id { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class JoinWebUrl
    {
        public string id { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventData
    {
        public bool isOnlineMeeting { get; set; }

        public string subject { get; set; }

        public Start start { get; set; }

        public End end { get; set; }

        public Organizer organizer { get; set; }

        public OnlineMeeting onlineMeeting { get; set; }

    }
    
    /// <summary>
    /// 
    /// </summary>
    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Organizer
    {
        public EmailAddress emailAddress { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OnlineMeeting
    {
        public string joinUrl { get; set; }
    }
}