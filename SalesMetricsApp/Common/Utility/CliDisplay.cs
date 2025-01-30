using SalesMetricsApp.Statistics.Model;

namespace SalesMetricsApp.Common.Utility
{
    public static class CliDisplay
    {
        public static string[] Commands = ["RESTART", "BACK", "EXIT"]; 

        public static void ShowBanner(string name, string version)
        {
            string label = $"====== {name} - {version} ======";
            string border = new string('=', label.Length);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(border);
            Console.WriteLine(label);
            Console.WriteLine(border);
            Console.ResetColor(); 
        }

        public static void ShowPageLabel(string label)
        {
            string border = new string('-', label.Length);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(label);
            Console.WriteLine(border);
            Console.ResetColor();
        }

        public static void ShowCommands()
        {
            string commands = string.Join("'] ['", Commands);
            string label = $"Commands: ['{commands}']";
            string border = new string('-', label.Length);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(label);
            Console.WriteLine(border);
            Console.ResetColor();
        }

        public static void ShowLine()
        {
            string border = new string('.', 80);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(border);
            Console.ResetColor();
        }

        public static void ShowRequest(string message, List<string>? menuItems = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            menuItems?
                .Select((item, index) => $"{index + 1}. {item}")
                .ToList()
                .ForEach(Console.WriteLine);
            Console.ResetColor();
        }

        public static void ShowStatisticResult(StatisticsResult statResult)
        {
            string countLog = $">> Total Entries: {statResult.TotalEntries}";
            string resultLog = $">> {statResult.Description}: {statResult.Result:F2}";

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(countLog);
            Console.WriteLine(resultLog);
            Console.ResetColor();
            CliDisplay.ShowLine();
        }

        public static void ShowInfo(string info, bool showLine = true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($">> {info}");
            Console.ResetColor();
            if (showLine) CliDisplay.ShowLine();
        }

        public static void ShowErrorMessage(string error)
        {         
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($">> {error}");
            Console.ResetColor();
            CliDisplay.ShowLine();
        }

        public static void ShowProgress(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"\r>> {message}");
        }
    }
}
