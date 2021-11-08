using System;
using Quartz;
using Quartz.Impl;

namespace AppCheckinLocation
{
    public class Scheduler
    {
        public void Start()
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<Job>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity("IDGJob", "IDG")
               .StartAt(DateBuilder.DateOf(16, 59, 0, 8, 11, 2021))
               .WithPriority(1)
               .Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
