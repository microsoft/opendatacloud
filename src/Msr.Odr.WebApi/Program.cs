using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Msr.Odr.Api
{
    /// <summary>
    /// Application entry point class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseAzureAppServices()
                .UseStartup<Startup>()
                .Build();
        }
    }
}
