using Quartz;
using System;
using System.Threading.Tasks;

namespace AppCheckinLocation
{
    public class Job: IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello Job");

            return null;
        }
    }
}
