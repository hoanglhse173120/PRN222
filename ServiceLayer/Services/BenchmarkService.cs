using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class BenchmarkService : IBenchmarkService
{
    private readonly IRepository<ExperimentConfig> _configRepo;
    private readonly IRepository<BenchmarkResult> _resultRepo;
    private readonly IRepository<TestQuestion> _questionRepo;

    public BenchmarkService(
        IRepository<ExperimentConfig> configRepo,
        IRepository<BenchmarkResult> resultRepo,
        IRepository<TestQuestion> questionRepo)
    {
        _configRepo = configRepo;
        _resultRepo = resultRepo;
        _questionRepo = questionRepo;
    }

    public async Task<IEnumerable<ExperimentConfig>> GetAllConfigsAsync()
        => await _configRepo.GetAllAsync();

    public async Task<ExperimentConfig> CreateConfigAsync(ExperimentConfig config)
    {
        config.CreatedAt = DateTime.Now;
        await _configRepo.AddAsync(config);
        await _configRepo.SaveChangesAsync();
        return config;
    }

    public async Task<IEnumerable<BenchmarkResult>> GetResultsByConfigAsync(int configId)
        => await _resultRepo.FindAsync(r => r.ConfigID == configId);

    public async Task<IEnumerable<TestQuestion>> GetAllQuestionsAsync()
        => await _questionRepo.GetAllAsync();

    public async Task AddQuestionAsync(TestQuestion question)
    {
        question.CreatedAt = DateTime.Now;
        await _questionRepo.AddAsync(question);
        await _questionRepo.SaveChangesAsync();
    }

    public async Task SaveBenchmarkResultAsync(BenchmarkResult result)
    {
        result.EvaluatedAt = DateTime.Now;
        await _resultRepo.AddAsync(result);
        await _resultRepo.SaveChangesAsync();
    }
}
