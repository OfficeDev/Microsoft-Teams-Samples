// <copyright file="CardTaskFetchValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace AppCompleteAuth.Models
{
    /// <summary>
    /// Card task fetch value model class.
    /// </summary>
    public class CardTaskFetchValue<T>
    {
        [JsonProperty("state")]
        public object State { get; set; }

        [JsonProperty("username")]
        public object UserName { get; set; }

        [JsonProperty("password")]
        public object Password { get; set; }
    }
}