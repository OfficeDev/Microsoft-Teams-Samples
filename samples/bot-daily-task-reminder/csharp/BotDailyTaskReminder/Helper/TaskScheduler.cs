// <copyright file="TaskScheduler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Quartz;
using Quartz.Impl;
using System;

namespace BotDailyTaskReminder
{
    public class TaskScheduler
    {
        // Method to schedule task.
        public void Start(int hour, int min, string baseUrl, DayOfWeek[] selectedDays)
        {
            try
            {
                // Synchronously get the scheduler and start it
                var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
                scheduler.Start();

                IJobDetail job = JobBuilder.Create<ScheduleTaskReminder>()
                                           .UsingJobData("baseUrl", baseUrl)
                                           .Build();

                // Create the cron schedule for the selected days and time
                CronScheduleBuilder csb = CronScheduleBuilder
                    .AtHourAndMinuteOnGivenDaysOfWeek(hour, min, selectedDays);

                ICronTrigger trigger = (ICronTrigger)TriggerBuilder
                    .Create()
                    .WithSchedule(csb)
                    .Build();

                // Schedule the job
                scheduler.ScheduleJob(job, trigger);
                Console.WriteLine("Task successfully scheduled.");
            }
            catch (SchedulerException ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Scheduler error: {ex.Message}");
            }
        }
    }
}
