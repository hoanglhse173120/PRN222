using ServiceLayer.DTOs;

namespace ServiceLayer.Interfaces;

public interface IBenchmarkService
{
    Task<IEnumerable<ExperimentConfigDto>> GetAllConfigsAsync();
    Task<ExperimentConfigDto> CreateConfigAsync(ExperimentConfigDto config);
    Task<IEnumerable<BenchmarkResultDto>> GetResultsByConfigAsync(int configId);
    Task<IEnumerable<TestQuestionDto>> GetAllQuestionsAsync();
    Task AddQuestionAsync(TestQuestionDto question);
    Task SaveBenchmarkResultAsync(BenchmarkResultDto result);
}
