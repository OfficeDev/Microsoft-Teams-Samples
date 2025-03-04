// <copyright file="TaskScheduler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Quartz;
using Quartz.Impl;
using System;

namespace BotDailyTaskReminder
{
    /// <summary>
    /// Class to handle task scheduling.
    /// </summary>
    public class TaskScheduler
    {
        /// <summary>
        /// Schedules a task to run at the specified time on the selected days.
        /// </summary>
        /// <param name="hour">The hour at which the task should run.</param>
        /// <param name="min">The minute at which the task should run.</param>
        /// <param name="baseUrl">The base URL for the task reminder API.</param>
        /// <param name="selectedDays">The days of the week on which the task should run.</param>
        public void Start(int hour, int min, string baseUrl, DayOfWeek[] selectedDays)
        {
            try
            {
                // Synchronously get the scheduler and start it
                var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
                scheduler.Start();

                var job = JobBuilder.Create<ScheduleTaskReminder>()
                                    .UsingJobData("baseUrl", baseUrl)
                                    .Build();

                // Create the cron schedule for the selected days and time
                var csb = CronScheduleBuilder
                    .AtHourAndMinuteOnGivenDaysOfWeek(hour, min, selectedDays);

                var trigger = (ICronTrigger)TriggerBuilder
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
