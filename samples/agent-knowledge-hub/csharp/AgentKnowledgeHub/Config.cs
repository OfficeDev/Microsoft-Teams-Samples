namespace AgentKnowledgeHub
{
    public class ConfigOptions
    {
        public string BOT_ID { get; set; }
        public string BOT_PASSWORD { get; set; }
        public string BOT_TYPE { get; set; }
        public string BOT_TENANT_ID { get; set; }
        public AzureConfigOptions Azure { get; set; }
    }

    /// <summary>
    /// Options for Azure OpenAI and Azure Content Safety
    /// </summary>
    public class AzureConfigOptions
    {
        public string OpenAIApiKey { get; set; }
        public string OpenAIEndpoint { get; set; }
        public string OpenAIDeploymentName { get; set; }
    }
}
