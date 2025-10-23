using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Common
{
    /// <summary>
    /// 工作流状态日志活动
    /// </summary>
    public class LogWorkflowStatusActivity : CodeActivity
    {
        [Input(Description = "步骤名称")]
        public Input<string> StepName { get; set; } = default!;

        [Input(Description = "状态消息")]
        public Input<string> StatusMessage { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<LogWorkflowStatusActivity>>();
            var stepName = StepName.Get(context);
            var statusMessage = StatusMessage.Get(context);

            var workflowInstanceId = context.WorkflowExecutionContext.Id;
            
            logger.LogInformation(
                "工作流状态更新 [WorkflowId: {WorkflowId}] - 步骤: {StepName}, 状态: {StatusMessage}",
                workflowInstanceId,
                stepName,
                statusMessage
            );

            await Task.CompletedTask;
        }
    }
}