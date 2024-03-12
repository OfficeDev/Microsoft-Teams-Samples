using Newtonsoft.Json;

namespace TeamsTalentMgmtApp.Models.Commands
{
    public class ActionCommandBase
    {
        [JsonProperty("commandId")]
        public string CommandId { get; set; }
    }
}