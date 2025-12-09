// <copyright file="CardTaskFetchValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace MessagingExtensionReminder.Models
{
    /// <summary>
    /// Card task fetch value model class.
    /// </summary>
    public class CardTaskFetchValue<T>
    {
        [JsonPropertyName("type")]
        public object Type { get; set; } = "task/fetch";

        [JsonPropertyName("id")]
        public object Id { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}