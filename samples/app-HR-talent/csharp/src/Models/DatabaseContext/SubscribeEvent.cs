namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class SubscribeEvent
    {
        public int SubscribeEventId { get; set; }

        public string WebhookUrl { get; set; }
    }
}
