using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Configuration;
using SalesMetricsApp.Common.Controller;
using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;
using SalesMetricsApp.Common.Core.UserPrompt.Service;
using SalesMetricsApp.Common.Utility;
using SalesMetricsApp.Controller.Model.Enum;
using SalesMetricsApp.FileGenerator.Service;

namespace SalesMetricsApp.FileGenerator
{
    public class GenerateFilesController : BaseController
    {
        private readonly ILogger<GenerateFilesController> _logger;
        private readonly IUserPromptService _userPromptService;
        private readonly IFileGeneratorService _fileGeneratorService;
        private readonly AppSettings _appSettings;

        public GenerateFilesController(
            ILogger<GenerateFilesController> logger,
            IUserPromptService userPromptService,
            IFileGeneratorService fileGeneratorService,
            AppSettings appSettings) : base(logger, userPromptService, appSettings)
        {
            _logger = logger;
            _userPromptService = userPromptService;
            _fileGeneratorService = fileGeneratorService;
            _appSettings = appSettings;
        }

        protected override void Execute()
        {
            var exportPath = _userPromptService.Prompt<string>(PromptType.ExportPath, "Pleaser provide Path to the Directory you want to Generate Files.");
            var dataDimensions = _userPromptService.Prompt<int[]>(PromptType.DataSize, "Pleaser enter Number of Files & then Number of Entries per File, separated by ','");

            _fileGeneratorService.Generate(exportPath, dataDimensions.First(), dataDimensions.Last());
        }

        public override void Refresh()
        {
            base.Refresh();
            CliDisplay.ShowPageLabel(_appSettings.PageHeaders[Page.GenerateFiles]);
        }
    }
}
