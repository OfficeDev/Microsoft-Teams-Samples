using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabActivityFeed.Model;
using TabActivityFeed.Repository;

namespace TabActivityFeed.Helpers
{
    public class TaskHelper
    {

        public static FeedRepository AddTaskToFeed(TaskDetails taskDetails)
        {
            FeedRepository.Tasks.Add(new TaskDetails
            {
                title = taskDetails.title,
                DeployementTitle = taskDetails.DeployementTitle,
                description = taskDetails.description,
                DeploymentDescription = taskDetails.DeploymentDescription
            });
            return null;
        }
    }
}
