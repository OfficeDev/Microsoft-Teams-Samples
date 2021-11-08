using System;
using Quartz;
using Quartz.Impl;

namespace AppCheckinLocation
{
    public class Scheduler
    {
        public void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<Job>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity("IDGJob", "IDG")
               .StartAt(DateBuilder.DateOf(20, 54, 0, 7, 11, 2021))
               .WithPriority(1)
               .Build();
            scheduler.ScheduleJob(job, trigger);

        }
    }
}
