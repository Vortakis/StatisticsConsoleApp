namespace SalesMetricsApp.Common.Core.FileSearch.Service
{
    public interface IFileSearchService
    {
        public bool Configure(string path, string extension);

        public Dictionary<DateTime, List<float>> Search(Func<KeyValuePair<DateTime, List<float>>, bool> filter);
    }
}
