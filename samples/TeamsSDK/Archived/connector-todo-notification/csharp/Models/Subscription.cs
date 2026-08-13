namespace TeamsToDoAppConnector.Models
{
    /// <summary>
    /// Represents the model to store channel subscriptions.
    /// </summary>
    public class Subscription
    {
        public string? WebHookUri { get; set; }
        public EventType EventType { get; set; }

        // Identity of the Entra ID user/connector context that registered this webhook.
        public string? OwnerObjectId { get; set; }
        public string? OwnerTenantId { get; set; }
    }

    /// <summary>
    /// Represents the event type which user is interested in getting notification for.
    /// </summary>
    public enum EventType
    {
        Create,
        Update
    }
}