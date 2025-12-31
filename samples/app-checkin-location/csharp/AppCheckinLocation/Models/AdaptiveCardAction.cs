// <copyright file="AdaptiveCardAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace AppCheckinLocation.Models
{
    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class AdaptiveCardAction
    {
        [JsonPropertyName("msteams")]
        public TaskAction MsteamsCardAction { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
    }

    public class TaskAction
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
