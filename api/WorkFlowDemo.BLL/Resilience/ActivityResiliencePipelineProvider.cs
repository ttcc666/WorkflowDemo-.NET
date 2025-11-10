using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using WorkFlowDemo.BLL.Activities.MaterialOutbound;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Resilience;

/// <summary>
/// Activity 弹性管道提供者，为不同的 Activity 提供预配置的 Polly 弹性策略
/// </summary>
public class ActivityResiliencePipelineProvider : IActivityResiliencePipelineProvider
{
    /// <summary>
    /// 缓存已创建的弹性管道，键为 (Activity类型, 结果类型)
    /// </summary>
    private readonly ConcurrentDictionary<(Type ActivityType, Type ResultType), object> _pipelines = new();
    private readonly ILogger<ActivityResiliencePipelineProvider> _logger;

    public ActivityResiliencePipelineProvider(ILogger<ActivityResiliencePipelineProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 尝试获取指定 Activity 类型的弹性管道
    /// </summary>
    public bool TryGetPipeline<TResult>(Type activityType, out ResiliencePipeline<TResult>? pipeline)
    {
        pipeline = GetOrCreatePipeline<TResult>(activityType);
        return pipeline is not null;
    }

    /// <summary>
    /// 获取或创建弹性管道，使用缓存避免重复创建
    /// </summary>
    private ResiliencePipeline<TResult>? GetOrCreatePipeline<TResult>(Type activityType)
    {
        var key = (activityType, typeof(TResult));
        if (_pipelines.TryGetValue(key, out var cached))
            return cached as ResiliencePipeline<TResult>;

        var pipeline = CreatePipeline<TResult>(activityType);
        if (pipeline is not null)
            _pipelines[key] = pipeline;

        return pipeline;
    }

    /// <summary>
    /// 根据 Activity 类型创建对应的弹性管道
    /// </summary>
    private ResiliencePipeline<TResult>? CreatePipeline<TResult>(Type activityType)
    {
        if (activityType == typeof(GetOutboundDetailsActivity))
            return (ResiliencePipeline<TResult>?)(object?)CreateOutboundDetailsPipeline();

        if (activityType == typeof(CheckInventoryActivity))
            return (ResiliencePipeline<TResult>?)(object?)CreateInventoryCheckPipeline();

        if (activityType == typeof(UpdateInventoryActivity))
            return (ResiliencePipeline<TResult>?)(object?)CreateInventoryUpdatePipeline();

        if (activityType == typeof(WriteHistoryActivity))
            return (ResiliencePipeline<TResult>?)(object?)CreateHistoryPipeline();

        if (activityType == typeof(DeleteScanRecordsActivity))
            return (ResiliencePipeline<TResult>?)(object?)CreateCleanupPipeline();

        return null;
    }

    /// <summary>
    /// 创建出库明细查询的弹性管道
    /// 策略：降级(空列表) -> 重试(3次,指数退避) -> 超时(3秒)
    /// </summary>
    private ResiliencePipeline<List<MaterialOutboundDetailDto>> CreateOutboundDetailsPipeline()
    {
        const string activity = nameof(GetOutboundDetailsActivity);
        var predicate = new PredicateBuilder<List<MaterialOutboundDetailDto>>()
            .Handle<TimeoutRejectedException>()
            .Handle<Exception>();

        return new ResiliencePipelineBuilder<List<MaterialOutboundDetailDto>>()
            .AddFallback(new FallbackStrategyOptions<List<MaterialOutboundDetailDto>>
            {
                ShouldHandle = predicate,
                FallbackAction = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 降级：回退到空的出库明细以触发审批路径。", activity);
                    return Outcome.FromResultAsValueTask(new List<MaterialOutboundDetailDto>());
                }
            })
            .AddRetry(new RetryStrategyOptions<List<MaterialOutboundDetailDto>>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 第 {Attempt} 次重试，原因：{Reason}", activity, args.AttemptNumber, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(3),
                OnTimeout = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 超时（{Timeout}s）", activity, args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// 创建库存检查的弹性管道
    /// 策略：降级(返回false) -> 重试(2次) -> 超时(2秒)
    /// </summary>
    private ResiliencePipeline<bool> CreateInventoryCheckPipeline()
    {
        const string activity = nameof(CheckInventoryActivity);
        var predicate = new PredicateBuilder<bool>()
            .Handle<TimeoutRejectedException>()
            .Handle<Exception>();

        return new ResiliencePipelineBuilder<bool>()
            .AddFallback(new FallbackStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                FallbackAction = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 降级：默认库存不足，进入人工审批。", activity);
                    return Outcome.FromResultAsValueTask(false);
                }
            })
            .AddRetry(new RetryStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(150),
                OnRetry = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 第 {Attempt} 次重试。", activity, args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(2),
                OnTimeout = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 检查库存超时（{Timeout}s）。", activity, args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// 创建库存更新的弹性管道
    /// 策略：降级(返回false) -> 熔断(失败率25%,采样15秒,熔断20秒) -> 重试(4次,指数退避) -> 超时(4秒)
    /// </summary>
    private ResiliencePipeline<bool> CreateInventoryUpdatePipeline()
    {
        const string activity = nameof(UpdateInventoryActivity);
        var predicate = new PredicateBuilder<bool>()
            .Handle<TimeoutRejectedException>()
            .Handle<Exception>();

        return new ResiliencePipelineBuilder<bool>()
            .AddFallback(new FallbackStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                FallbackAction = args =>
                {
                    _logger.LogError(args.Outcome.Exception, "[Resilience] {Activity} 降级：库存扣减失败，返回 false 等待补偿。", activity);
                    return Outcome.FromResultAsValueTask(false);
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                BreakDuration = TimeSpan.FromSeconds(20),
                FailureRatio = 0.25,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(15),
                OnOpened = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 熔断开启，暂停 {Break}s。", activity, args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("[Resilience] {Activity} 熔断关闭，恢复服务。", activity);
                    return ValueTask.CompletedTask;
                }
            })
            .AddRetry(new RetryStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(250),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 第 {Attempt} 次重试，原因：{Reason}", activity, args.AttemptNumber, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(4),
                OnTimeout = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 执行超时（{Timeout}s）。", activity, args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// 创建履历写入的弹性管道
    /// 策略：降级(空列表) -> 重试(3次,指数退避) -> 超时(3秒)
    /// </summary>
    private ResiliencePipeline<List<string>> CreateHistoryPipeline()
    {
        const string activity = nameof(WriteHistoryActivity);
        var predicate = new PredicateBuilder<List<string>>()
            .Handle<TimeoutRejectedException>()
            .Handle<Exception>();

        return new ResiliencePipelineBuilder<List<string>>()
            .AddFallback(new FallbackStrategyOptions<List<string>>
            {
                ShouldHandle = predicate,
                FallbackAction = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 降级：写入履历失败，返回空集合等待补偿。", activity);
                    return Outcome.FromResultAsValueTask(new List<string>());
                }
            })
            .AddRetry(new RetryStrategyOptions<List<string>>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 第 {Attempt} 次重试。", activity, args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(3),
                OnTimeout = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 写入履历超时（{Timeout}s）。", activity, args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// 创建清理扫描记录的弹性管道
    /// 策略：降级(返回false) -> 重试(3次) -> 超时(2秒)
    /// </summary>
    private ResiliencePipeline<bool> CreateCleanupPipeline()
    {
        const string activity = nameof(DeleteScanRecordsActivity);
        var predicate = new PredicateBuilder<bool>()
            .Handle<TimeoutRejectedException>()
            .Handle<Exception>();

        return new ResiliencePipelineBuilder<bool>()
            .AddFallback(new FallbackStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                FallbackAction = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 降级：删除扫描记录失败，返回 false 以便后续清理任务重试。", activity);
                    return Outcome.FromResultAsValueTask(false);
                }
            })
            .AddRetry(new RetryStrategyOptions<bool>
            {
                ShouldHandle = predicate,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(150),
                OnRetry = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 第 {Attempt} 次重试。", activity, args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(2),
                OnTimeout = args =>
                {
                    _logger.LogWarning("[Resilience] {Activity} 清理超时（{Timeout}s）。", activity, args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
