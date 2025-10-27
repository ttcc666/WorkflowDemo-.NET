using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Common
{
    [Activity("WorkflowDemo", "Common", "记录工作流状态")]
    public class LogWorkflowStatusActivity : CodeActivity
    {
        [Input(Description = "步骤名称")]
        public Input<string> StepName { get; set; } = default!;

        [Input(Description = "状态消息")]
        public Input<string> StatusMessage { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<LogWorkflowStatusActivity>>();
            logger.LogInformation("工作流状态 [Id: {Id}] - 步骤: {Step}, 状态: {Status}",
                context.WorkflowExecutionContext.Id,
                StepName.GetOrDefault(context) ?? "未知",
                StatusMessage.GetOrDefault(context) ?? "无");
            await Task.CompletedTask;
        }
    }
}