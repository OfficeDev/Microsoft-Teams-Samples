namespace ReporterPlus.Models
{
    public class MessagingExtensionActionDeserializer
    {
        public string commandId { get; set; }
        public string commandContext { get; set; }
        public string requestId { get; set; }
        public ContextType context { get; set; }
    }

    public class ContextType
    {
        public string theme { get; set; }
    }

}
