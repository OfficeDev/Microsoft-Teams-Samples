// <copyright file="AppInstallDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace QRAppInstallation.Models
{
    /// <summary>
    /// App install details model class.
    /// </summary>
    public class AppInstallDetails<T>
    {

        [JsonProperty("appid")]
        public object AppId { get; set; }

        [JsonProperty("teamid")]
        public object TeamId { get; set; }
    }
}
