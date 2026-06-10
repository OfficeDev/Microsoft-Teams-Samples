using System;
using Newtonsoft.Json;

namespace TeamsTalentMgmtApp.Models.Commands
{
    public sealed class PositionCreateCommand : ActionCommandBase
    {
        [JsonProperty("positionId")]
        public int PositionId { get; set; }

        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }

        [JsonProperty("jobLevel")]
        public int JobLevel { get; set; }

        [JsonProperty("jobPostingDate")]
        public DateTimeOffset JobPostingDate { get; set; }

        [JsonProperty("jobLocation")]
        public int JobLocation { get; set; }

        [JsonProperty("jobDescription")]
        public string JobDescription { get; set; }

        [JsonProperty("jobHiringManager")]
        public string JobHiringManager { get; set; }
    }
}