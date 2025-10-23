using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities
{
    /// <summary>
    /// 模拟失败活动 - 用于测试补偿机制
    /// </summary>
    public class SimulateFailureActivity : CodeActivity
    {
        [Input(Description = "是否模拟失败")]
        public Input<bool> ShouldFail { get; set; } = default!;

        [Input(Description = "失败消息")]
        public Input<string> FailureMessage { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<SimulateFailureActivity>>();
            var shouldFail = ShouldFail.Get(context);
            var failureMessage = FailureMessage.Get(context);

            if (shouldFail)
            {
                logger.LogError("模拟失败: {FailureMessage}", failureMessage);
                throw new Exception(failureMessage);
            }

            logger.LogInformation("模拟失败检查通过,继续执行");
            await Task.CompletedTask;
        }
    }
}