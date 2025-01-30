using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Utility;
using System.Text;

namespace SalesMetricsApp.FileGenerator.Service

{
    public class FileGeneratorService : IFileGeneratorService
    {
        private readonly ILogger<FileGeneratorService> _logger;

        private readonly Random _random;
        private readonly string fileNamePrefix = "SalesData_";
        private readonly string fileExtension = ".txt";
        private readonly string[] dateFormats =
        [
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "yy.MM.dd"
        ];

        public FileGeneratorService(ILogger<FileGeneratorService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public void Generate(string path, int fileCount, int entryCount)
        {
            int totalCount = fileCount * entryCount;
            int generated = 0;

            _logger.LogInformation($"Generating {totalCount} Sale Entries within {fileCount} Files.");

            int lastProgress = 0;
            Parallel.For(1, fileCount + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, fileId =>
            {
                string fileName = Path.Combine(path, $"{fileNamePrefix}{fileId}{fileExtension}");

                using (StreamWriter writer = new StreamWriter(new BufferedStream(File.Open(fileName, FileMode.Create)), Encoding.UTF8, 8192))
                {
                    for (int entryId = 1; entryId <= entryCount; entryId++)
                    {
                        string date = GenerateRandomDate();
                        string amount = GenerateRandomAmount();

                        writer.WriteLine($"{date}##{amount}");
                        Interlocked.Increment(ref generated);

                        int progress = (generated * 100) / totalCount;
                        if (progress > lastProgress)
                        {
                            CliDisplay.ShowProgress($"Created: {((generated/(float)totalCount)*100):F1}% of entries");
                            lastProgress = progress;
                        }
                    }
                }
            });
            CliDisplay.ShowProgress($"Created: {((generated / (float)totalCount) * 100):F1}% of entries");

            Console.ResetColor();
            Console.WriteLine();
            _logger.LogInformation($"Generating files Completed.");
        }

        private string GenerateRandomDate()
        {
            int year = _random.Next(2010, 2025);
            int month = _random.Next(1, 13);
            int day = _random.Next(1, DateTime.DaysInMonth(year, month) + 1);
            int index = _random.Next(dateFormats.Length);

            return new DateTime(year, month, day).ToString(dateFormats[index]);
        }

        private string GenerateRandomAmount()
        {
            return _random.Next(10, 5000) + "." + _random.Next(0, 100).ToString("D2");
        }
    }
}
