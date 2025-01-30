using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Configuration;
using SalesMetricsApp.Common.Controller;
using SalesMetricsApp.Common.Core.FileSearch.Service;
using SalesMetricsApp.Common.Core.UserPrompt.Service;
using SalesMetricsApp.Controller.Model.Enum;
using SalesMetricsApp.FileGenerator;
using SalesMetricsApp.FileGenerator.Service;
using SalesMetricsApp.Home;
using SalesMetricsApp.Statistics;
using SalesMetricsApp.Statistics.Service;

namespace SalesMetricsApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            Settings settings = configuration.Get<Settings>();


            var serviceProvider = new ServiceCollection()
                .AddSingleton(settings.AppSettings)
                .AddLogging(config =>
                {
                    if (settings.Logging.EnableDebugLogs)
                    {
                        config.AddSimpleConsole(options =>
                        {
                            options.IncludeScopes = false;
                            options.SingleLine = false;
                            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                            options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                        });
                    }
                })
                .AddSingleton<Router>()
                .AddSingleton<IUserPromptService, UserPromptService>()
                .AddSingleton<IFileSearchService, FileSearchService>()
                .AddSingleton<IStatisticsService, StatisticsService>()
                .AddTransient<HomeController>()
                .AddTransient<StatisticsController>()
                .AddTransient<GenerateFilesController>()
                .AddTransient<IFileGeneratorService, FileGeneratorService>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var router = serviceProvider.GetRequiredService<Router>();
            router.Register(Page.Home, serviceProvider.GetRequiredService<HomeController>());
            router.Register(Page.Statistics, serviceProvider.GetRequiredService<StatisticsController>());
            router.Register(Page.GenerateFiles, serviceProvider.GetRequiredService<GenerateFilesController>());

            logger.LogInformation("Application starting...");
            router.NavigateTo(Page.Home);
            logger.LogInformation("Application finished.");
        }
    }
}


