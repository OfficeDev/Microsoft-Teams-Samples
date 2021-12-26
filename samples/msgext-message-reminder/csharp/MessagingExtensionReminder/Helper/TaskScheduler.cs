// <copyright file="TaskScheduler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Quartz;
using Quartz.Impl;

namespace MessagingExtensionReminder
{
    public class TaskScheduler
    {
        // Method to schedule task.
        public void Start(int year, int month, int day, int hour, int min, string baseUrl)
        {
            var triggerName = Guid.NewGuid().ToString();
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<ScheduleTaskReminder>().
                                    UsingJobData("baseUrl", baseUrl).
                                    Build();

            ITrigger trigger = TriggerBuilder.Create()
                                     .WithIdentity(triggerName, triggerName)
                                     .StartAt(DateBuilder.DateOf(hour, min, 0, day, month, year))
                                     .WithPriority(1)
                                     .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}