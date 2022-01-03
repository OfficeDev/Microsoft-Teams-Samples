// <copyright file="TokenState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright

using Newtonsoft.Json;

namespace JoinTeamByQR.Models
{
    /// <summary>
    /// Class with taskmodule response model.
    /// </summary>
    public class ResponseData<T>
    {
        [JsonProperty("teamId")]
        public object TeamId { get; set; }

        [JsonProperty("userId")]
        public object UserId { get; set; }
    }
}