using SalesMetricsApp.Common.Core.FileSearch.Service;
using SalesMetricsApp.Statistics.Model;

namespace SalesMetricsApp.Statistics.Service
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IFileSearchService _fileSearchService;

        public StatisticsService(
            IFileSearchService fileSearchService)
        {
            _fileSearchService = fileSearchService;
        }

        public StatisticsResult Average(DateTime startDate, DateTime endDate)
        {
            StatisticsResult statisticsResult = new StatisticsResult();
            var entries = _fileSearchService.Search(kv => kv.Key >= startDate && kv.Key >= endDate);

            if (!entries.Any())
                return statisticsResult;

            var flattenEntries = GetValuesOnly(entries);

            statisticsResult.Result = flattenEntries.Average();
            statisticsResult.TotalEntries = flattenEntries.Count();

            return statisticsResult;
        }

        public StatisticsResult StandardDeviation(DateTime date)
        {
            StatisticsResult statisticsResult = new StatisticsResult();
            var entries = _fileSearchService.Search(kv => kv.Key == date);

            if (!entries.Any())
                return statisticsResult;

            var flattenEntries = GetValuesOnly(entries);

            float average = flattenEntries.Average();
            statisticsResult.Result = (float)Math.Sqrt(flattenEntries.Average(v => Math.Pow(v - average, 2)));
            statisticsResult.TotalEntries = flattenEntries.Count();

            return statisticsResult;
        }

        public StatisticsResult StandardDeviation(DateTime startDate, DateTime endDate)
        {
            StatisticsResult statisticsResult = new StatisticsResult();
            var entries = _fileSearchService.Search(kv => kv.Key >= startDate && kv.Key >= endDate);

            if (!entries.Any())
                return statisticsResult;

            var flattenEntries = GetValuesOnly(entries);

            float average = flattenEntries.Average();
            statisticsResult.Result = (float)Math.Sqrt(flattenEntries.Average(v => Math.Pow(v - average, 2)));
            statisticsResult.TotalEntries = flattenEntries.Count();

            return statisticsResult;
        }

        private IEnumerable<float> GetValuesOnly(Dictionary<DateTime, List<float>> entries)
        {
            return entries.SelectMany(kv => kv.Value);
        }

        
    }
}
