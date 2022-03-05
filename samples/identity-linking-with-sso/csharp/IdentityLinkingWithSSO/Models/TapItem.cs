// <copyright file="ListCard.cs" company="UTI">
// Copyright (c) UTI. All rights reserved.
// </copyright>

namespace IdentityLinkingWithSSO.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represents list card model.
    /// </summary>
    public class TapItem
    {
        /// <summary>
        /// Gets or sets title of list card.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets items of list card.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}