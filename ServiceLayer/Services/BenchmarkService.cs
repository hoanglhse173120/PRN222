using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using ServiceLayer.Interfaces;
using ServiceLayer.DTOs;

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

    public async Task<IEnumerable<ExperimentConfigDto>> GetAllConfigsAsync()
    {
        var configs = await _configRepo.GetAllAsync();
        return configs.Select(c => new ExperimentConfigDto
        {
            ConfigId = c.ConfigId,
            ConfigName = c.ConfigName,
            ApproachType = c.ApproachType,
            EmbeddingModel = c.EmbeddingModel,
            ChunkingStrategy = c.ChunkingStrategy,
            ChunkSize = c.ChunkSize,
            ChunkOverlap = c.ChunkOverlap,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<ExperimentConfigDto> CreateConfigAsync(ExperimentConfigDto configDto)
    {
        var config = new ExperimentConfig
        {
            ConfigName = configDto.ConfigName,
            ApproachType = configDto.ApproachType,
            EmbeddingModel = configDto.EmbeddingModel,
            ChunkingStrategy = configDto.ChunkingStrategy,
            ChunkSize = configDto.ChunkSize,
            ChunkOverlap = configDto.ChunkOverlap,
            CreatedAt = DateTime.Now
        };

        await _configRepo.AddAsync(config);
        await _configRepo.SaveChangesAsync();
        
        configDto.ConfigId = config.ConfigId;
        configDto.CreatedAt = config.CreatedAt;
        return configDto;
    }

    public async Task<IEnumerable<BenchmarkResultDto>> GetResultsByConfigAsync(int configId)
    {
        var results = await _resultRepo.FindAsync(r => r.ConfigId == configId);
        return results.Select(r => new BenchmarkResultDto
        {
            ResultId = r.ResultId,
            ConfigId = r.ConfigId,
            QuestionId = r.QuestionId,
            ModelResponse = r.ModelResponse,
            Faithfulness = r.Faithfulness,
            AnswerRelevance = r.AnswerRelevance,
            ContextPrecision = r.ContextPrecision,
            ContextRecall = r.ContextRecall,
            EvaluatedAt = r.EvaluatedAt
        });
    }

    public async Task<IEnumerable<TestQuestionDto>> GetAllQuestionsAsync()
    {
        var questions = await _questionRepo.GetAllAsync();
        return questions.Select(q => new TestQuestionDto
        {
            QuestionId = q.QuestionId,
            SubjectId = q.SubjectId,
            QuestionText = q.QuestionText,
            GroundTruth = q.GroundTruth,
            CreatedAt = q.CreatedAt
        });
    }

    public async Task AddQuestionAsync(TestQuestionDto questionDto)
    {
        var question = new TestQuestion
        {
            SubjectId = questionDto.SubjectId,
            QuestionText = questionDto.QuestionText,
            GroundTruth = questionDto.GroundTruth,
            CreatedAt = DateTime.Now
        };
        await _questionRepo.AddAsync(question);
        await _questionRepo.SaveChangesAsync();
    }

    public async Task SaveBenchmarkResultAsync(BenchmarkResultDto resultDto)
    {
        var result = new BenchmarkResult
        {
            ConfigId = resultDto.ConfigId,
            QuestionId = resultDto.QuestionId,
            ModelResponse = resultDto.ModelResponse,
            Faithfulness = resultDto.Faithfulness,
            AnswerRelevance = resultDto.AnswerRelevance,
            ContextPrecision = resultDto.ContextPrecision,
            ContextRecall = resultDto.ContextRecall,
            EvaluatedAt = DateTime.Now
        };
        await _resultRepo.AddAsync(result);
        await _resultRepo.SaveChangesAsync();
    }
}
