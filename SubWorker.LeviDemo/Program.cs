using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SubWorker.LeviDemo
{
    class Program
    {
        private static void Main()
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<LeviSubWorkerBackgroundService>();
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }
}