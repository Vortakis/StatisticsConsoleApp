using SalesMetricsApp.Statistics.Model;

namespace SalesMetricsApp.Statistics.Service
{
    public interface IStatisticsService
    {
        StatisticsResult Average(DateTime startDate, DateTime endDate);

        StatisticsResult StandardDeviation(DateTime date);

        StatisticsResult StandardDeviation(DateTime startDate, DateTime endDate);
    }
}
