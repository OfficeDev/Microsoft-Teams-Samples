// <copyright file="ScheduleTaskReminder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Quartz;
using System.Net.Http;
using System.Threading.Tasks;

namespace MessagingExtensionReminder
{
    public class ScheduleTaskReminder : IJob
    {
        static HttpClient client = new HttpClient();
        
        // Method to execute scheduled job.
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string baseUrl = dataMap.GetString("baseUrl");

            await client.GetAsync(baseUrl + "/api/task");
        }
    }
}