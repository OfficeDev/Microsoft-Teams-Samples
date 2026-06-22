namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class ConversationData
    {
        public int ConversationDataId { get; set; }

        public int RecruiterId { get; set; }

        public string AccountId { get; set; }

        public string ServiceUrl { get; set; }

        public string TenantId { get; set; }
    }
}
