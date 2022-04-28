using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MSGraphSearchSample.Models
{
    public class CardTaskSubmitValue<T>
    {
        [JsonProperty("type")]
        public object Type { get; set; } = "task/fetch";

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("submitType")]
        public string SubmitType { get; set; }
    }
}
