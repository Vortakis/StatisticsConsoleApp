using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Utility;
using SalesMetricsApp.Controller.Model.Enum;

namespace SalesMetricsApp.Common.Controller
{
    public class Router
    {
        private readonly ILogger<Router> _logger;
        private readonly Dictionary<Page, IController> _controllers = new();
        private readonly Stack<Page> _history = new();

        public Router (ILogger<Router> logger)
        {
            _logger = logger;
        }

        public void Register(Page page, IController controller)
        {
            _controllers[page] = controller;
            _logger.LogInformation($"'{this.GetType().Name}' controller registered.");
        }

        public void NavigateTo(Page page)
        {
            if (_controllers.TryGetValue(page, out var controller))
            {
                _history.Push(page);
                controller.Start();
            }
        }

        public bool HandleCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            input = input.Trim().ToUpper();

            if (!CliDisplay.Commands.Contains(input))
            {
                return false;
            }

            switch (input)
            {
                case "EXIT":
                    Environment.Exit(0);
                    return true;
                case "BACK":
                    return GoBack();
                case "RESTART":
                    Restart();
                    return true;
                default:
                    return false;
            }
        }

        private bool GoBack()
        {
            if (_history.Count > 1)
            {
                _history.Pop();
                NavigateTo(_history.Peek());
                return true;
            }
            else return false;
        }

        public void Restart()
        {
            var page = _history.Peek();
            if (_controllers.TryGetValue(page, out var controller))
            {
                controller.Start();
            }
        }
    }
}
