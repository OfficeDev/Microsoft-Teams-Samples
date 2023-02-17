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

        public static FeedRepository AddTaskToFeed(TaskDetails taskModuleData)
        {
            FeedRepository.Tasks.Add(new TaskDetails
            {
                title = taskModuleData.title,
                DeployementTitle = taskModuleData.DeployementTitle,
                description = taskModuleData.description,
                DeploymentDescription = taskModuleData.DeploymentDescription
            });
            return null;
        }
    }
}
