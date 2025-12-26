// <copyright file="TaskDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace MessagingExtensionReminder.Models
{
    /// <summary>
    /// Location details model class.
    /// </summary>
    public class TaskDetails
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("dateTime")]
        public DateTime? DateTime { get; set; }
    }
}
