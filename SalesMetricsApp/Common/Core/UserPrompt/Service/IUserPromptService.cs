using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;

namespace SalesMetricsApp.Common.Core.UserPrompt.Service
{
    public interface IUserPromptService
    {
        public T Prompt<T>(PromptType promptType, string message, List<string>? menuItems = null);
    }
}
