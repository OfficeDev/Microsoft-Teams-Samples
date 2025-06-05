// <copyright file="ScheduleTaskReminder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Quartz;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotDailyTaskReminder
{
    /// <summary>
    /// Class to handle scheduled task reminders.
    /// </summary>
    public class ScheduleTaskReminder : IJob
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Executes the scheduled job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var baseUrl = dataMap.GetString("baseUrl");

            // Validate baseUrl
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl), "Base URL must be provided in JobDataMap.");
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
