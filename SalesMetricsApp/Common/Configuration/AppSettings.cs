
using SalesMetricsApp.Controller.Model.Enum;

namespace SalesMetricsApp.Common.Configuration
{
    public class AppSettings
    {
        public string AppName { get; set; }
        public string Version { get; set; }
        public Dictionary<Page, string> PageHeaders { get; set; }
    }
}
