// <copyright file="ResponseEventData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscriptRecording.Models
{
    /// <summary>
    /// Represents a response containing a list of EventData objects.
    /// </summary>
    public class ResponseEventData
    {
        public List<EventData> Value { get; set; }
    }

    /// <summary>
    /// Represents a response containing a list of JoinWebUrl objects.
    /// </summary>
    public class JoinUrlData
    {
        public List<JoinWebUrl> Value { get; set; }
    }

    /// <summary>
    /// Represents a response containing a list of transcriptsId objects.
    /// </summary>
    public class transcriptsData
    {
        public List<transcriptsId> Value { get; set; }
    }

    /// <summary>
    /// Represents an object with a single property, id.
    /// </summary>
    public class transcriptsId
    {
        public string id { get; set; }
    }

    /// <summary>
    /// Represents a response containing a list of RecordingId objects.
    /// </summary>
    public class RecordingData
    {
        public List<RecordingId> Value { get; set; }
    }

    /// <summary>
    ///  Represents an object with a single property, id.
    /// </summary>
    public class RecordingId
    {
        public string id { get; set; }
    }

    /// <summary>
    ///  Represents an object with a single property, id.
    /// </summary>
    public class JoinWebUrl
    {
        public string id { get; set; }
    }

    /// <summary>
    /// Represents data related to an event, potentially an online meeting.
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
    /// Represents an email address, with name and address properties.
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