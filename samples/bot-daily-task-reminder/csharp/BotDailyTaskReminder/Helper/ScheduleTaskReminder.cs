// <copyright file="ScheduleTaskReminder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Quartz;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotDailyTaskReminder
{
    public class ScheduleTaskReminder : IJob
    {
        static HttpClient client = new HttpClient();

        // Method to execute scheduled job.
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string baseUrl = dataMap.GetString("baseUrl");

            // Validate baseUrl
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException("baseUrl", "Base URL must be provided in JobDataMap.");
            }

            try
            {
                // Make the HTTP request
                var response = await client.GetAsync(baseUrl + "/api/task");

                // Ensure the response indicates success (2xx status code)
                response.EnsureSuccessStatusCode();

                // Optionally log the success or handle the response
                Console.WriteLine($"Task reminder successfully triggered. Response: {response.StatusCode}");
            }
            catch (HttpRequestException e)
            {
                // Log the error (consider using a proper logging framework)
                Console.WriteLine($"Request failed: {e.Message}");

                // Handle the error (e.g., retry, send failure notification, etc.)
            }
        }
    }
}
