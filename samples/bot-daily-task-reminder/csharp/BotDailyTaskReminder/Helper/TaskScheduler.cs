// <copyright file="TaskScheduler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Quartz;
using Quartz.Impl;

namespace BotDailyTaskReminder
{
    public class TaskScheduler
    {
        // Method to schedule task.
        public void Start(int hour, int min, string baseUrl,string selectedDays)
        {
            var cronExpression = $"0 {min} {hour} ? * {selectedDays}";
            var triggerName = Guid.NewGuid().ToString();
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<ScheduleTaskReminder>().
                                    UsingJobData("baseUrl", baseUrl).
                                    Build();
            ITrigger trigger = TriggerBuilder.Create()
                                    .ForJob(job)
                                    .WithCronSchedule(cronExpression)
                                    .WithIdentity(triggerName)
                                    .StartNow()
                                    .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}