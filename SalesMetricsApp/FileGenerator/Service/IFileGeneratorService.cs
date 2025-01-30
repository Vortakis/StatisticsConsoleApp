namespace SalesMetricsApp.FileGenerator.Service
{
    public interface IFileGeneratorService
    {
        public void Generate(string path, int fileCount, int entryCount);
    }
}
