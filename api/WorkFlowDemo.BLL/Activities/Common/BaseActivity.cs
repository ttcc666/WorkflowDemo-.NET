using Elsa.Workflows;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Common
{
    public abstract class BaseActivity<TResult> : CodeActivity<TResult>
    {
        protected abstract ValueTask<TResult> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger);

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<BaseActivity<TResult>>>();
            try
            {
                var result = await ExecuteActivityAsync(context, logger);
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