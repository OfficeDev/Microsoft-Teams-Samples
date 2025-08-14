using System;

namespace TabActivityFeed.Model
{
    
    public class TaskDetails
    {
        public Guid taskId { get; set; }

        public string taskName { get; set; }

        public string taskItemLink { get; set; }

        public string chatId { get; set; }

        public string channelId { get; set; }

        public string teamId { get; set; }

        public string tenantId { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string userName { get; set; }

        public string status { get; set; }

        public string DeployementTitle { get; set; }

        public string DeploymentDescription { get; set; }
        public string taskInfoAction { get; set; }

        public string reservationId { get; set; }
        public string currentSlot { get; set; }
        public string reservationTitle { get; set; }

        public string access_token { get; set; }
    }
}
