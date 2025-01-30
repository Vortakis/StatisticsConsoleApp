using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Configuration;
using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;
using SalesMetricsApp.Common.Core.UserPrompt.Service;
using SalesMetricsApp.Common.Utility;

namespace SalesMetricsApp.Common.Controller
{
    public abstract class BaseController : IController
    {
        private readonly ILogger<BaseController> _logger;
        private readonly IUserPromptService _userPromptService;
        private readonly AppSettings _appSettings;

        public BaseController(
            ILogger<BaseController> logger,
            IUserPromptService userPromptService,
            AppSettings appSettings)
        {
            _logger = logger;
            _userPromptService = userPromptService;
            _appSettings = appSettings;
        }

        public void Start()
        {
            _logger.LogInformation($"Controller '{this.GetType().Name}' started ...");
            Refresh();
            Execute();
            _logger.LogInformation($"Controller '{this.GetType().Name}' ended.");

            _userPromptService.Prompt<string>(PromptType.Command, "Please type a [Command] to proceed...");
        }

        public virtual void Refresh()
        {
            Console.Clear();
            CliDisplay.ShowBanner(_appSettings.AppName, _appSettings.Version);
            CliDisplay.ShowCommands();
        }

        protected abstract void Execute();
    }
}
