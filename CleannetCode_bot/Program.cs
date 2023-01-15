using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleannetCode_bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                })
                .ConfigureServices((context,services) =>
                {
                    services.AddScoped<IBotService,BotService>();
                    services.AddScoped<IStorageService,StorageFileService>();
                    services.AddScoped<Handlers>();
                })
                .ConfigureLogging((logging) =>
                    logging.ClearProviders()
                        .AddConsole())
                .Build();

            var svc = ActivatorUtilities.CreateInstance<BotService>(host.Services);
            await svc.RunAsync();
        }
    }
}