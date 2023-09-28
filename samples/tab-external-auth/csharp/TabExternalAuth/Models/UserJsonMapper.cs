// <copyright file="UserJsonMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace TabExternalAuth.Models
{
    /// <summary>
    /// Model to map response json to user object.
    /// </summary>
    public class UserJsonMapper<T>
    {
        /// <summary>
        /// User's display name.
        /// </summary>
        [JsonProperty("names")]
        public object Names { get; set; }

        /// <summary>
        /// User's profile photo.
        /// </summary>
        [JsonProperty("photos")]
        public object Photos { get; set; }

        /// <summary>
        /// User's email address.
        /// </summary>
        [JsonProperty("emailAddresses")]
        public object EmailAddresses { get; set; }
    }
}