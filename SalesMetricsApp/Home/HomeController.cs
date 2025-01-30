using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Configuration;
using SalesMetricsApp.Common.Controller;
using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;
using SalesMetricsApp.Common.Core.UserPrompt.Service;
using SalesMetricsApp.Common.Utility;
using SalesMetricsApp.Controller.Model.Enum;

namespace SalesMetricsApp.Home
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserPromptService _userPromptService;
        private readonly Router _router;
        private readonly AppSettings _appSettings;

        public HomeController(
            ILogger<HomeController> logger,
            IUserPromptService userPromptService,
            Router router,
            AppSettings appSettings) : base(logger, userPromptService, appSettings)
        {
            _logger = logger;
            _userPromptService = userPromptService;
            _router = router;
            _appSettings = appSettings;
        }

        protected override void Execute()
        {
            List<string> menuItems = new List<string>
            {
                "Calculate Statistics",
                "Generate Dummy Sales Entry Files"
            };
            var menuChoice = _userPromptService.Prompt<int>(PromptType.Menu, $"Choose option between [1-{menuItems.Count}]:", menuItems);

            switch (menuChoice)
            {
                case 1:
                    _router.NavigateTo(Page.Statistics);
                    break;
                case 2:
                    _router.NavigateTo(Page.GenerateFiles);
                    break;
                default:
                    return;
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            CliDisplay.ShowPageLabel(_appSettings.PageHeaders[Page.Home]);
        }

    }
}
