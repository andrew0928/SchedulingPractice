using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace SubWorker.ChachaDemo
{
      class Program
        {
            static void Main(string[] args)
            {
                var host = new HostBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        //services.AddHostedService<AndrewSubWorkerBackgroundService>();
                        services.AddHostedService<ChachaSubWorkerBackgroundService>();
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