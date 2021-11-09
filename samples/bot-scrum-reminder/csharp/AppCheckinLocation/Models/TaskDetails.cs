// <copyright file="LocationDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace AppCheckinLocation.Models
{
    /// <summary>
    /// Location details model class.
    /// </summary>
    public class TaskDetails<T>
    {
        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("dateTime")]
        public object DateTime { get; set; }
    }
}
