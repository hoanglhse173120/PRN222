using DataAccessLayer.Models;

namespace ServiceLayer.Interfaces;

public interface IBenchmarkService
{
    Task<IEnumerable<ExperimentConfig>> GetAllConfigsAsync();
    Task<ExperimentConfig> CreateConfigAsync(ExperimentConfig config);
    Task<IEnumerable<BenchmarkResult>> GetResultsByConfigAsync(int configId);
    Task<IEnumerable<TestQuestion>> GetAllQuestionsAsync();
    Task AddQuestionAsync(TestQuestion question);
    Task SaveBenchmarkResultAsync(BenchmarkResult result);
}
