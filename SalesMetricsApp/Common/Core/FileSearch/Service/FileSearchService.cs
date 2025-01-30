using Microsoft.Extensions.Logging;
using SalesMetricsApp.Common.Core.FileSearch.Model.Enum;
using SalesMetricsApp.Common.Utility;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace SalesMetricsApp.Common.Core.FileSearch.Service
{
    public class FileSearchService : IFileSearchService
    {
        private readonly ILogger<FileSearchService> _logger;

        private SortedDictionary<DateTime, List<float>> _repository = new();
        private SearchStrategy _strategy;
        private string? _path;
        private string? _extension;

        public FileSearchService(ILogger<FileSearchService> logger)
        {
            _logger = logger;
        }

        public bool Configure(string path, string extension)
        {
            _path = path;
            _extension = extension;
            var files = Directory.GetFiles(path, $"*{extension}");

            if (files.Length < 1)
            {
                CliDisplay.ShowErrorMessage($"Invalid Input. Directory does not contain '{extension}' files.");
                return false;
            }

            long totalSizeBytes = files.Sum(f => new FileInfo(f).Length);
            long totalSizeMB = totalSizeBytes / (1024 * 1024);

            if (totalSizeMB <= 1000)
            {
                _strategy = SearchStrategy.InMemory;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                InMemory_PreLoadData(totalSizeMB > 50);
                stopwatch.Stop();

                CliDisplay.ShowInfo($"Total File Size: {totalSizeMB} MB, Pre-Load Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
                _logger.LogInformation($"TotalSize: {totalSizeMB}MB & Pre-LoadTime: {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            }
            else
            {
                _strategy = SearchStrategy.OnDemand;
                CliDisplay.ShowInfo($"Total File Size: {totalSizeMB} MB");
                _logger.LogInformation($"TotalSize: {totalSizeMB}MB.");
            }
            _logger.LogInformation($"File Search configured Path: '{_path}', Type: '{_extension}' files and will use Search Strategy: '{SearchStrategy.OnDemand}'.");

            return true;
        }

        public Dictionary<DateTime, List<float>> Search(Func<KeyValuePair<DateTime, List<float>>, bool> filter)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = _strategy switch
            {
                SearchStrategy.None => throw new Exception("SearchService not configured."),
                SearchStrategy.InMemory => InMemory_Search(filter),
                SearchStrategy.OnDemand => OnDemand_Search(filter),
                _ => throw new NotImplementedException()
            };

            stopwatch.Stop();

            CliDisplay.ShowInfo($"Search Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            _logger.LogInformation($"Search Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");

            return result;
        }

        private Dictionary<DateTime, List<float>> InMemory_Search(Func<KeyValuePair<DateTime, List<float>>, bool> filter)
        {
            var result = _repository
                .Where(filter)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return result;
        }

        private Dictionary<DateTime, List<float>> OnDemand_Search(Func<KeyValuePair<DateTime, List<float>>, bool> filter)
        {
            var result = new ConcurrentDictionary<DateTime, List<float>>();
            var files = Directory.GetFiles(_path, $"*{_extension}");
            int filesCounter = 0;
            int totalFiles = files.Count();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Parallel.ForEach(files, file =>
            {
                ProcessFile(file, result, filter);
                Interlocked.Increment(ref filesCounter);
                CliDisplay.ShowProgress($"On-demand Scan: {filesCounter / (float)totalFiles * 100:F1}%");
            });
            Console.WriteLine();
            Console.ResetColor();
            CliDisplay.ShowLine();

            return new Dictionary<DateTime, List<float>>(result);
        }
        private void InMemory_PreLoadData(bool useParallel)
        {
            var tempData = new ConcurrentDictionary<DateTime, List<float>>();
            var files = Directory.GetFiles(_path, $"*{_extension}");
            int filesCounter = 0;
            int totalFiles = files.Count();
            Console.ForegroundColor = ConsoleColor.Magenta;

            if (useParallel)
            {
                Parallel.ForEach(files, file =>
                {
                    ProcessFile(file, tempData);
                    Interlocked.Increment(ref filesCounter);
                    CliDisplay.ShowProgress($"In-memory Scan: {filesCounter / (float)totalFiles * 100:F1}%");
                });
            }
            else
            {
                foreach (var file in files)
                {
                    ProcessFile(file, tempData);
                    filesCounter++;
                    CliDisplay.ShowProgress($"In-memory Scan: {filesCounter / (float)totalFiles * 100:F1}%");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            CliDisplay.ShowLine();
            _repository = new SortedDictionary<DateTime, List<float>>(tempData);
        }

        private void ProcessFile(string file, IDictionary<DateTime, List<float>> data, Func<KeyValuePair<DateTime, List<float>>, bool> filter = null)
        {
            foreach (var line in File.ReadLines(file).Select((content, index) => new { content, index }))
            {
                if (!TryParseLine(line.content, out var date, out var amount))
                {
                    // Use index of line and file name to take logs of problematic lines in files if required.
                    continue;
                }

                var entry = new KeyValuePair<DateTime, List<float>>(date, new List<float> { amount });

                if (filter == null || filter(entry))
                {
                    if (data is ConcurrentDictionary<DateTime, List<float>> concurrentDict)
                    {
                        concurrentDict.AddOrUpdate(date,
                            _ => new List<float> { amount },
                            (_, list) =>
                            {
                                lock (list) { list.Add(amount); }
                                return list;
                            });
                    }
                    else
                    {
                        if (!data.ContainsKey(date))
                            data[date] = new List<float>();
                        data[date].Add(amount);
                    }
                }
            }
        }

        private bool TryParseLine(string line, out DateTime date, out float amount)
        {
            date = default;
            amount = default;

            var parts = line.Split("##");
            if (parts.Length != 2)
                return false;

            return DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out date) &&
                   float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out amount);
        }
    }
}
