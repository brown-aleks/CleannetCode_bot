using CleannetCode_bot.Features.Welcome;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CleannetCode_bot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    var env = hostingContext.HostingEnvironment;

                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<IBotService, BotService>();
                    services.AddSingleton<WelcomeHandler>();
                    services.AddScoped<IStorageService, StorageFileService>();
                    services.AddScoped<Handlers>();
                    services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
                    {
                        var accessToken = context.Configuration.GetValue<string>("AccessToken")!;
                        return new(accessToken);
                    });
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