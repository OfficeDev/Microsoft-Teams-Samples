// <copyright file="CardTaskFetchValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace IdentityLinkingWithSSO.Models
{
    /// <summary>
    /// Card task fetch value model class.
    /// </summary>
    public class CardTaskFetchValue<T>
    {
        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("state")]
        public object State { get; set; }

        [JsonProperty("username")]
        public object UserName { get; set; }

        [JsonProperty("password")]
        public object Password { get; set; }

        [JsonProperty("names")]
        public object Names { get; set; }

        [JsonProperty("photos")]
        public object Photos { get; set; }

        [JsonProperty("emailAddresses")]
        public object EmailAddresses { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}