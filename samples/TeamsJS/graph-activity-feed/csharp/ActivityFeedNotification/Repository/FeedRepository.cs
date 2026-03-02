using System.Collections.Generic;
using TabActivityFeed.Model;

namespace TabActivityFeed.Repository
{
    public class FeedRepository
    {
        public static List<TaskDetails> Tasks { get; set; } = new List<TaskDetails>();
        static FeedRepository()
        {
            Tasks.Add(new TaskDetails
            {
                title = "Get the bills",
                DeployementTitle = "Get the bills",
                description = "Get the travel and accommodation bills",
                DeploymentDescription = "Get the travel and accommodation bills"
            });

            Tasks.Add(new TaskDetails
            {
                title = "Get the pay",
                DeployementTitle = "Get the pay",
                description = "Get the travel and accommodation slips",
                DeploymentDescription = "Get the travel and accommodation slips"
            });
        }
    }
}
