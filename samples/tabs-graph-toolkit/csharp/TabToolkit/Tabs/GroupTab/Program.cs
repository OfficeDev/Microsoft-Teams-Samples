using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Tabs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string directory = Directory.GetCurrentDirectory() + "\\GroupTab";
            CreateWebHostBuilder(args).UseContentRoot(directory).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
