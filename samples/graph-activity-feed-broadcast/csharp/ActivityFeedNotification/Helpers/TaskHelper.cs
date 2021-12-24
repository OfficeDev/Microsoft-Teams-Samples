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

        public static FeedRepository AddTaskToFeed(TaskInfo taskInfo){
            FeedRepository.Tasks.Add(new TaskInfo
            {
                title = taskInfo.title,
                DeployementTitle = taskInfo.DeployementTitle,
                description = taskInfo.description,
                DeploymentDescription = taskInfo.DeploymentDescription
            });
            return null;
        }
    }
}
