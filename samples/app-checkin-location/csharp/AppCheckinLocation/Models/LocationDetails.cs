// <copyright file="LocationDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace AppCheckinLocation.Models
{
    /// <summary>
    /// Location details model class.
    /// </summary>
    public class LocationDetails<T>
    {
        [JsonPropertyName("latitude")]
        public object Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public object Longitude { get; set; }
    }
}
