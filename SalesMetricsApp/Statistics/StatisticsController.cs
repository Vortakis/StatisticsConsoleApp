using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Configuration;
using SalesMetricsApp.Common.Controller;
using SalesMetricsApp.Common.Core.FileSearch.Service;
using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;
using SalesMetricsApp.Common.Core.UserPrompt.Service;
using SalesMetricsApp.Common.Utility;
using SalesMetricsApp.Controller.Model.Enum;
using SalesMetricsApp.Statistics.Model;
using SalesMetricsApp.Statistics.Service;

namespace SalesMetricsApp.Statistics
{
    public class StatisticsController : BaseController
    {
        private readonly ILogger<StatisticsController> _logger;
        private readonly IUserPromptService _userPromptService;
        private readonly IFileSearchService _fileSearchService;
        private readonly IStatisticsService _statisticsService;

        private readonly AppSettings _appSettings;

        private string importPath = string.Empty;
        private string fileExt = string.Empty;
        private string dateFormat = "dd/MM/yy";

        public StatisticsController(
            ILogger<StatisticsController> logger,
            IUserPromptService userPromptService,
            IFileSearchService fileSearchService,
            IStatisticsService statisticsService,
            AppSettings appSettings) : base(logger, userPromptService, appSettings)
        {
            _logger = logger;
            _userPromptService = userPromptService;
            _fileSearchService = fileSearchService;
            _statisticsService = statisticsService;
            _appSettings = appSettings;
        }

        protected override void Execute()
        {
            ConfigFileSearch();

            List<string> menuItems = new List<string>
            {
                "Average of Earnings for: Range of Years",
                "Standard Deviation of Earnings for: Specific Year",
                "Standard Deviation of Earnings for: Range of Years"
            };

            var menuChoice = _userPromptService.Prompt<int>(PromptType.Menu, $"Choose option between [1-{menuItems.Count}]:", menuItems);
            StatisticsResult result;
            switch (menuChoice)
            {
                case 1:
                    var dateRangeAver = _userPromptService.Prompt<DateTime[]>(PromptType.DateRange, "Enter StartDate and EndDate, separated by 'to':");
                    result = _statisticsService.Average(dateRangeAver[0], dateRangeAver[1]);
                    result.Description = $"Average between {dateRangeAver[0].ToString(dateFormat)} to {dateRangeAver[1].ToString(dateFormat)}";
                    break;
                case 2:
                    var dateSD = _userPromptService.Prompt<DateTime>(PromptType.Date, $"Enter Specific Date:");
                    result = _statisticsService.StandardDeviation(dateSD);
                    result.Description = $"Standard Deviation on {dateSD.ToString(dateFormat)}";

                    break;
                case 3:
                    var dateRangeSD = _userPromptService.Prompt<DateTime[]>(PromptType.DateRange, "Enter StartDate and EndDate, separated by 'to':");
                    result = _statisticsService.StandardDeviation(dateRangeSD[0], dateRangeSD[1]);
                    result.Description = $"Standard Deviation between {dateRangeSD[0].ToString(dateFormat)} to {dateRangeSD[1].ToString(dateFormat)}";

                    break;
                default: 
                    return;
            }

            CliDisplay.ShowStatisticResult(result);
        }

        public override void Refresh()
        {
            base.Refresh();
            CliDisplay.ShowPageLabel(_appSettings.PageHeaders[Page.Statistics]);
        }

        private void ConfigFileSearch()
        {
            bool configFiles = true;

            if (!string.IsNullOrEmpty(importPath) && !string.IsNullOrEmpty(fileExt))
            {
                configFiles = false;

                CliDisplay.ShowInfo($"File Directory: {importPath}", false);
                CliDisplay.ShowInfo($"File Extension: {fileExt}");

                configFiles = _userPromptService.Prompt<bool>(PromptType.YesNo, $"Want to read Directory again? (Recommended if you believe files changed)");
                if (configFiles)
                {
                    configFiles = !_userPromptService.Prompt<bool>(PromptType.YesNo, $"Want to use same Dictory and File Extension?");
                    if (!configFiles)
                    {
                        configFiles = !_fileSearchService.Configure(importPath, fileExt);
                    }
                }
            }

            while (configFiles)
            {
                importPath = _userPromptService.Prompt<string>(PromptType.ImportPath, $"Paste Path to directory with Sales Files:");
                fileExt = _userPromptService.Prompt<string>(PromptType.FileExt, $"Enter the file extension to search for the corresponding files (e.g. '.txt'):");
                configFiles = !_fileSearchService.Configure(importPath, fileExt);
            }
        } 
    }
}
