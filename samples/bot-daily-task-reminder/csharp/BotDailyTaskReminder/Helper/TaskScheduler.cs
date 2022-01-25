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
        public void Start(int hour, int min, string baseUrl, DayOfWeek[] selectedDays)
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<ScheduleTaskReminder>().
                                    UsingJobData("baseUrl", baseUrl).
                                    Build();

            CronScheduleBuilder csb = CronScheduleBuilder
                .AtHourAndMinuteOnGivenDaysOfWeek(hour, min, selectedDays);

            ICronTrigger trigger = (ICronTrigger)TriggerBuilder
                .Create()
                .WithSchedule(csb)
                .Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}