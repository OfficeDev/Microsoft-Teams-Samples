using Quartz;
using System;

namespace AppCheckinLocation
{
    public class Job: IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello task");
        }
    }
}
