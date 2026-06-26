using System.Collections.Generic;
using TeamsToDoAppConnector.Models;

namespace TeamsToDoAppConnector.Repository
{
    /// <summary>
    /// Represents the subscription repository class which stores the temporary data.
    /// </summary>
    public class SubscriptionRepository
    {
        public static List<Subscription> Subscriptions { get; set; } = new List<Subscription>() ;
    }
}