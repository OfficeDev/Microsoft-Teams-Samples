// <copyright file="MeetingDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using System.Collections.Generic;

namespace MeetingLiveCoding.Models
{
    /// <summary>
    /// Meeting details model class.
    /// </summary>
    public class MeetingDetails
    {
        public string MeetingId { get; set; }
        public List<Question> Questions { get; set; }
    }
}