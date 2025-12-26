// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace JoinTeamByQR.Models
{
    /// <summary>
    /// Class with taskmodule response model.
    /// </summary>
    public class ResponseData
    {
        [JsonProperty("teamId")]
        public string TeamId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
