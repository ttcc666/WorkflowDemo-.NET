using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Resilience;

namespace WorkFlowDemo.BLL.Activities.Common
{
    public abstract class BaseActivity<TResult> : CodeActivity<TResult>
    {
        protected abstract ValueTask<TResult> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger);

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<BaseActivity<TResult>>>();
            var pipelineProvider = context.GetRequiredService<IActivityResiliencePipelineProvider>();
            // 将所有节点包装进 Polly Pipeline，便于按活动类型切换重试/熔断/降级策略。
            try
            {
                TResult result;
                if (pipelineProvider.TryGetPipeline<TResult>(GetType(), out var pipeline) && pipeline is not null)
                {
                    result = await pipeline.ExecuteAsync(async token =>
                    {
                        token.ThrowIfCancellationRequested();
                        return await ExecuteActivityAsync(context, logger);
                    }, context.CancellationToken);
                }
                else
                {
                    result = await ExecuteActivityAsync(context, logger);
                }
                context.Set(Result, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Activity执行失败: {ActivityType}", GetType().Name);
                throw;
            }
        }
    }
}
