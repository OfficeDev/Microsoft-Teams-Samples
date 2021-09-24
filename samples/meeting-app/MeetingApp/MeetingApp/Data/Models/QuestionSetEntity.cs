using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Models
{
    /// <summary>
    /// Class for Question details related properties.
    /// </summary>
    public class QuestionSetEntity: TableEntity
    {
        [JsonProperty("meetingId")]
        public string MeetingId { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("questionId")]
        public string QuestionId { get; set; }

        [JsonProperty("setBy")]
        public string SetBy { get; set; }

        [JsonProperty("isDelete")]
        public int IsDelete { get; set; }
    }
}
